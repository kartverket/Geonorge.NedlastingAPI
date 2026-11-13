package download.altinn

import rego.v1

pdp_url := "http://altinn-pdp.tilgangsstyring-altinn:8080/authorize"

# Ansattporten `acr` -> Altinn authentication level, per the altinn-pdp OpenAPI spec.
acr_levels := {
	"substantial": 3,
	"high": 4,
}

# Organisation numbers the user represents for `resource`, taken from Ansattporten `authorization_details`.
org_numbers(claims, resource) := {org |
	some detail in object.get(claims, "authorization_details", [])
	detail.resource == resource
	some party in object.get(detail, "authorized_parties", [])
	org := org_number(party.orgno.ID)
}

# "0192:314246756" -> "314246756"
org_number(actor_id) := org if {
	is_string(actor_id)
	parts := split(actor_id, ":")
	org := parts[count(parts) - 1]
	regex.match(`^[0-9]{9}$`, org)
}

pdp_permit(request, acr) if {
	response := http.send({
		"method": "POST",
		"url": pdp_url,
		"headers": {"Content-Type": "application/json"},
		"body": request,
		"timeout": "10s",
		"raise_error": false,
	})
	response.status_code == 200
	response.body.permit == true
	auth_level_sufficient(object.get(response.body, "minimumAuthenticationLevel", null), acr)
}

# The PDP does not enforce `minimumAuthenticationLevel` itself; the caller must.
auth_level_sufficient(null, _)

auth_level_sufficient(min_level, acr) if {
	is_number(min_level)
	acr_levels[acr] >= min_level
}
