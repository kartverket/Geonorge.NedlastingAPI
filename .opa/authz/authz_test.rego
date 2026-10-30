package authz_test

import rego.v1

import data.authz

valid_claims := {
	"active": true,
	"aud": authz.expected_audience,
	"client": allowed_client,
}

allowed_client := min(authz.allowed_clients)

introspect_ok(body) := {"status_code": 200, "body": body}

mock_valid(req) := introspect_ok(valid_claims) if {
	req.method == "POST"
	req.url == authz.introspection_url
	req.body == {"identity_provider": "tokenx", "token": "good-token"}
}

mock_inactive(_) := introspect_ok({"active": false})

mock_wrong_aud(_) := introspect_ok(object.union(valid_claims, {"aud": "other:ns:app"}))

mock_aud_list(_) := introspect_ok(object.union(valid_claims, {"aud": ["other:ns:app", authz.expected_audience]}))

mock_wrong_client(_) := introspect_ok(object.union(valid_claims, {"client": "other:ns:app"}))

mock_missing_client(_) := introspect_ok(object.remove(valid_claims, {"client"}))

mock_server_error(_) := {"status_code": 500, "body": {}}

test_allow_valid_token if {
	authz.allow with input as {"token": "good-token"} with http.send as mock_valid
}

test_allow_aud_as_list if {
	authz.allow with input as {"token": "t"} with http.send as mock_aud_list
}

test_deny_inactive_token if {
	not authz.allow with input as {"token": "t"} with http.send as mock_inactive
}

test_deny_wrong_audience if {
	not authz.allow with input as {"token": "t"} with http.send as mock_wrong_aud
}

test_deny_wrong_client if {
	not authz.allow with input as {"token": "t"} with http.send as mock_wrong_client
}

test_deny_missing_client if {
	not authz.allow with input as {"token": "t"} with http.send as mock_missing_client
}

test_deny_introspection_error if {
	not authz.allow with input as {"token": "t"} with http.send as mock_server_error
}

test_deny_missing_token if {
	not authz.allow with input as {} with http.send as mock_valid
}

test_deny_empty_token if {
	not authz.allow with input as {"token": ""} with http.send as mock_valid
}
