package authz

import rego.v1

introspection_url := "http://localhost:3000/api/v1/introspect"

# This API's TokenX identity.
expected_audience := "atkv3-dev:datadeling-geonorge:geonorge-nedlasting-api"

# Applications permitted to call this API (matched against the `client` claim).
allowed_clients := {"geonorge-ny-kartkatalog-dev:kartkatalog-backend"}

default allow := false

allow if {
	claims.active == true
	audience_valid
	claims.client in allowed_clients
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
