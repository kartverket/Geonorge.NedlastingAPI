package download_test

import rego.v1

import data.download
import data.download.altinn

valid_claims := {
	"active": true,
	"aud": download.expected_audience,
	"client_id": allowed_client,
	"pid": "15819099887",
	"acr": "high",
	"authorization_details": [{
		"resource": download.altinn_main_resource,
		"authorized_parties": [
			{"orgno": {"authority": "iso6523-actorid-upis", "ID": "0192:314246756"}},
			{"orgno": {"authority": "iso6523-actorid-upis", "ID": "0192:111111111"}},
		],
	}],
}

allowed_client := min(download.allowed_clients)

introspect_ok(body) := {"status_code": 200, "body": body}

mock_valid(req) := introspect_ok(valid_claims) if {
	req.method == "POST"
	req.url == download.introspection_url
	req.body == {"identity_provider": "tokenx", "token": "good-token"}
}

mock_inactive(_) := introspect_ok({"active": false})

mock_wrong_aud(_) := introspect_ok(object.union(valid_claims, {"aud": "other:ns:app"}))

mock_aud_list(_) := introspect_ok(object.union(valid_claims, {"aud": ["other:ns:app", download.expected_audience]}))

mock_wrong_client(_) := introspect_ok(object.union(valid_claims, {"client_id": "other:ns:app"}))

mock_missing_client(_) := introspect_ok(object.remove(valid_claims, {"client_id"}))

mock_server_error(_) := {"status_code": 500, "body": {}}

# Introspection succeeds; PDP permits only the expected request for org 314246756.
mock_full(req) := introspect_ok(valid_claims) if req.url == download.introspection_url

mock_full(req) := {"status_code": 200, "body": {"permit": permitted}} if {
	req.url == altinn.pdp_url
	permitted := req.body == {
		"pid": "15819099887",
		"resourceId": "kv-geonorge",
		"customerOrganizationNumber": "314246756",
		"action": "download",
		"resourceAttributes": [{"type": "urn:kv:geonorge", "id": "dataset"}],
	}
}

mock_pdp_deny(req) := introspect_ok(valid_claims) if req.url == download.introspection_url

mock_pdp_deny(req) := {"status_code": 200, "body": {"permit": false}} if req.url == altinn.pdp_url

mock_invalid_token(req) := introspect_ok({"active": false}) if req.url == download.introspection_url

mock_invalid_token(req) := {"status_code": 200, "body": {"permit": true}} if req.url == altinn.pdp_url

test_valid_token if {
	download.valid_token with input as {"token": "good-token"} with http.send as mock_valid
}

test_valid_token_aud_as_list if {
	download.valid_token with input as {"token": "t"} with http.send as mock_aud_list
}

test_token_invalid_inactive if {
	not download.valid_token with input as {"token": "t"} with http.send as mock_inactive
}

test_token_invalid_wrong_audience if {
	not download.valid_token with input as {"token": "t"} with http.send as mock_wrong_aud
}

test_token_invalid_wrong_client if {
	not download.valid_token with input as {"token": "t"} with http.send as mock_wrong_client
}

test_token_invalid_missing_client if {
	not download.valid_token with input as {"token": "t"} with http.send as mock_missing_client
}

test_token_invalid_introspection_error if {
	not download.valid_token with input as {"token": "t"} with http.send as mock_server_error
}

test_token_invalid_missing_token if {
	not download.valid_token with input as {} with http.send as mock_valid
}

test_token_invalid_empty_token if {
	not download.valid_token with input as {"token": ""} with http.send as mock_valid
}

test_allow_when_pdp_permits if {
	download.can_order with input as {"token": "t"} with http.send as mock_full
}

test_permitted_org_numbers_only_pdp_permitted if {
	orgs := download.permitted_org_numbers with input as {"token": "t"} with http.send as mock_full
	orgs == {"314246756"}
}

test_deny_when_pdp_denies if {
	not download.can_order with input as {"token": "t"} with http.send as mock_pdp_deny
}

test_deny_when_token_invalid_even_if_pdp_permits if {
	not download.can_order with input as {"token": "t"} with http.send as mock_invalid_token
}

test_decision_valid_and_permitted if {
	download.decision == {"valid_token": true, "can_order": true, "role_sufficient": false} with input as {"token": "t"} with http.send as mock_full
}

test_decision_valid_but_denied if {
	download.decision == {"valid_token": true, "can_order": false, "role_sufficient": false} with input as {"token": "t"} with http.send as mock_pdp_deny
}

test_decision_invalid_token if {
	download.decision == {"valid_token": false, "can_order": false, "role_sufficient": false} with input as {"token": "t"} with http.send as mock_invalid_token
}

test_role_sufficient_when_all_have_role_or_null if {
	download.role_sufficient with input as {"requiredRoles": {"a": {"role": ["nd.filnedlasting"]}, "b": {"role": null}, "c": {}}}
}

test_role_sufficient_when_role_is_empty_list if {
	download.role_sufficient with input as {"requiredRoles": {"a": {"role": []}, "b": {"role": ["nd.filnedlasting"]}}}
}

test_role_sufficient_with_array_of_requirements if {
	download.role_sufficient with input as {"requiredRoles": [{"role": ["nd.filnedlasting", "other"]}]}
}

test_role_insufficient_when_one_lacks_role if {
	not download.role_sufficient with input as {"requiredRoles": {"a": {"role": ["nd.filnedlasting"]}, "b": {"role": ["other"]}}}
}

test_role_insufficient_when_empty_list_and_other_lacks_role if {
	not download.role_sufficient with input as {"requiredRoles": {"a": {"role": []}, "b": {"role": ["other"]}}}
}

test_role_insufficient_when_required_roles_missing if {
	not download.role_sufficient with input as {}
}

test_role_insufficient_when_required_roles_null if {
	not download.role_sufficient with input as {"requiredRoles": null}
}
