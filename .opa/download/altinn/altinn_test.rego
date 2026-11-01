package download.altinn_test

import rego.v1

import data.download.altinn

resource := "urn:altinn:resource:kv-geonorge"

claims := {"authorization_details": [
	{
		"resource": resource,
		"authorized_parties": [
			{"orgno": {"authority": "iso6523-actorid-upis", "ID": "0192:314246756"}},
			{"orgno": {"authority": "iso6523-actorid-upis", "ID": "0192:not-a-number"}},
		],
	},
	{
		"resource": "urn:altinn:resource:other",
		"authorized_parties": [{"orgno": {"ID": "0192:999999999"}}],
	},
]}

pdp_request := {
	"pid": "15819099887",
	"resourceType": "urn:kv:geonorge",
	"resourceId": "dataset",
	"customerOrganizationNumber": "314246756",
	"action": "download",
}

mock_permit(req) := {"status_code": 200, "body": {"permit": true, "decision": "PERMIT"}} if {
	req.method == "POST"
	req.url == altinn.pdp_url
	req.body == pdp_request
}

mock_deny(_) := {"status_code": 200, "body": {"permit": false, "decision": "DENY"}}

mock_permit_level_4(_) := {"status_code": 200, "body": {"permit": true, "minimumAuthenticationLevel": 4}}

mock_error(_) := {"status_code": 500, "body": {}}

test_org_numbers_only_for_requested_resource if {
	altinn.org_numbers(claims, resource) == {"314246756"}
}

test_org_numbers_empty_without_authorization_details if {
	altinn.org_numbers({}, resource) == set()
}

test_pdp_permit if {
	altinn.pdp_permit(pdp_request, "high") with http.send as mock_permit
}

test_pdp_deny if {
	not altinn.pdp_permit(pdp_request, "high") with http.send as mock_deny
}

test_pdp_error if {
	not altinn.pdp_permit(pdp_request, "high") with http.send as mock_error
}

test_pdp_permit_when_auth_level_sufficient if {
	altinn.pdp_permit(pdp_request, "high") with http.send as mock_permit_level_4
}

test_pdp_deny_when_auth_level_too_low if {
	not altinn.pdp_permit(pdp_request, "substantial") with http.send as mock_permit_level_4
}

test_pdp_deny_when_acr_unknown if {
	not altinn.pdp_permit(pdp_request, "low") with http.send as mock_permit_level_4
}
