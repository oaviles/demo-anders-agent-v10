# Service Principal Permissions Guide for Azure AI Foundry Deployments

This guide documents a generic RBAC model for deploying agents to Azure AI Foundry with a service principal.

It is designed for CI/CD workflows (GitHub Actions, Azure DevOps, etc.) that authenticate with Azure using:

1. `clientId`
2. `clientSecret`
3. `tenantId`
4. `subscriptionId`

## Why This Matters

Agent deployment operations require specific data-plane and control-plane permissions.

A common failure is:

`HTTP 403 Forbidden` for `Microsoft.CognitiveServices/accounts/AIServices/agents/write`

When this happens, authentication is valid, but authorization is insufficient at the target scope.

## Identity Model

For workflow-based deployments, Azure Login typically uses a service principal.

Permission checks are evaluated against the service principal object ID (not only the app/client ID).

## Recommended RBAC Strategy

Use least privilege first, then broaden only if required.

### Minimum Practical Scope

Assign permissions at the Azure AI Foundry project scope:

`/subscriptions/<subscriptionId>/resourceGroups/<rg>/providers/Microsoft.CognitiveServices/accounts/<account>/projects/<project>`

### Role Options

Use one of these roles depending on your organization policy:

1. `Foundry Owner` (project scope) - preferred for project-level deployment control.
2. `Foundry Account Owner` (account scope or higher) - broader Foundry control.
3. `Cognitive Services Contributor` (account scope) - useful fallback for account-level operations.

`Contributor` alone may still be insufficient for some Foundry agent operations, depending on service enforcement and scope.

## Scope Hierarchy (Narrow to Broad)

1. Project scope (preferred)
2. Account scope
3. Resource group scope
4. Subscription scope

Start narrow and escalate only when required.

## CLI: Create Role Assignments

Replace placeholders before running.

```bash
ASSIGNEE_OBJECT_ID="<service-principal-object-id>"
SUBSCRIPTION_ID="<subscription-id>"
RESOURCE_GROUP="<resource-group>"
ACCOUNT_NAME="<ai-services-account>"
PROJECT_NAME="<foundry-project>"

PROJECT_SCOPE="/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.CognitiveServices/accounts/$ACCOUNT_NAME/projects/$PROJECT_NAME"
ACCOUNT_SCOPE="/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.CognitiveServices/accounts/$ACCOUNT_NAME"

# Preferred: project scope
az role assignment create \
  --assignee-object-id "$ASSIGNEE_OBJECT_ID" \
  --assignee-principal-type ServicePrincipal \
  --role "Foundry Owner" \
  --scope "$PROJECT_SCOPE"

# Optional fallback: account scope
az role assignment create \
  --assignee-object-id "$ASSIGNEE_OBJECT_ID" \
  --assignee-principal-type ServicePrincipal \
  --role "Cognitive Services Contributor" \
  --scope "$ACCOUNT_SCOPE"
```

## CLI: Verify Effective Assignments

```bash
az role assignment list \
  --assignee-object-id "<service-principal-object-id>" \
  --all \
  -o table
```

You should see expected roles at the intended scope path.

## Endpoint Alignment Check

The Foundry endpoint in your secret must match the same account/project where RBAC was granted:

`https://<account>.services.ai.azure.com/api/projects/<project>`

If the endpoint points to a different account, project, tenant, or subscription, role assignments will not apply.

## GitHub Secret Example

Use a JSON object in `AZURE_CREDENTIALS`:

```json
{
  "clientId": "<app-client-id>",
  "clientSecret": "<app-client-secret>",
  "subscriptionId": "<subscription-id>",
  "tenantId": "<tenant-id>"
}
```

Also ensure your workflow uses the correct Foundry endpoint secret and model deployment variables.

## Troubleshooting Checklist

If deployment fails with `403` for `agents/write`:

1. Confirm the object ID in the error is the same principal used by workflow credentials.
2. Confirm role assignment exists at project/account/scope aligned with endpoint.
3. Confirm endpoint project belongs to the same tenant and subscription as credentials.
4. Wait for RBAC propagation (usually a few minutes).
5. Re-run workflow after propagation.
6. Check for policy or deny assignments if your organization uses governance restrictions.

## Security Best Practices

1. Rotate `clientSecret` periodically.
2. Rotate immediately after accidental exposure.
3. Prefer OIDC/federated credentials where possible to avoid long-lived client secrets.
4. Keep scope minimal (project first).
5. Audit role assignments regularly.

## Repository Usage Notes

For this repository, deployment scripts and workflow expect:

1. Foundry project endpoint (`FOUNDRY_PROJECT_ENDPOINT`)
2. Model deployment name (`FOUNDRY_MODEL_DEPLOYMENT_NAME`)

Use this guide whenever onboarding a new environment, service principal, or subscription.
