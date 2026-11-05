package download

import rego.v1

import data.download.altinn

introspection_url := "http://localhost:3000/api/v1/introspect"

# This API's TokenX identity.
expected_audience := "atkv3-dev:datadeling-geonorge:geonorge-nedlasting-api"

# Applications permitted to call this API (matched against the `client_id` claim).
allowed_clients := {"atkv3-dev:geonorge-ny-kartkatalog-dev:kartkatalog-backend"}

# Altinn main resource granted at Ansattporten login.
altinn_main_resource := "urn:altinn:resource:kv-geonorge"

# Main resource with sub resource attributes, checked against the Altinn PDP.
pdp_resource := {
	"resourceId": "kv-geonorge",
	"action": "download",
	"resourceAttributes": [{"type": "urn:kv:geonorge", "id": "dataset"}],
}

# Lets the backend tell an invalid token (401) from a denied order (403) in one query.
decision := {
	"valid_token": valid_token,
	"can_order": can_order,
	"role_sufficient": role_sufficient
}

default can_order := false

can_order if count(permitted_org_numbers) > 0

# Organisations on whose behalf the user may perform `pdp_resource`.
permitted_org_numbers contains org if {
	valid_token
	some org in altinn.org_numbers(claims, altinn_main_resource)
	request := object.union(pdp_resource, {"pid": claims.pid, "customerOrganizationNumber": org})
	altinn.pdp_permit(request, claims.acr)
}

default valid_token := false

valid_token if {
	claims.active == true
	audience_valid
	claims.client_id in allowed_clients
}

# `aud` may be a single string or a list.
audience_valid if claims.aud == expected_audience

audience_valid if {
	is_array(claims.aud)
	expected_audience in claims.aud
}

claims := response.body if {
	is_string(input.token)
	input.token != ""
	response := http.send({
		"method": "POST",
		"url": introspection_url,
		"headers": {"Content-Type": "application/json"},
		"body": {
			"identity_provider": "tokenx",
			"token": input.token,
		},
		"raise_error": false,
	})
	response.status_code == 200
}

default role_sufficient := false

# Every required entry either has no `role` (missing, null or empty) or lists "nd.filnedlasting".
role_sufficient if {
	input.requiredRoles != null
	every _, required in input.requiredRoles {
		role_allows_download(required)
	}
}

role_allows_download(required) if object.get(required, "role", null) == null

# The backend sends an empty list for datasets without a required role.
role_allows_download(required) if required.role == []

role_allows_download(required) if "nd.filnedlasting" in required.role





