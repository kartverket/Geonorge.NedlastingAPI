# OPA Policies

This directory contains OPA (Open Policy Agent) policies for testing and development purposes.

## Structure

- `download/download.rego` - Main download authorization policy
- `.manifest` - OPA bundle manifest

## Building the Bundle

To build an OPA bundle from this directory:

```bash
opa build -b .opa -o bundle.tar.gz
```

## Testing Policies

To test the policies locally:

```bash
# Run OPA with the policy
opa run --server .opa/

# Test a request
curl -X POST http://localhost:8181/v1/data/download/can_order \
  -H "Content-Type: application/json" \
  -d '{"input": {"token": "<access-token>"}}'
```
