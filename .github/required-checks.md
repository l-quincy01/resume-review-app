# Required Checks Before Merge

Enable branch protection for `main` in GitHub repository settings and require these checks before merging:

- `CI / install-build-lint-typecheck-test`
- `API / api-build-test`

Recommended protection settings:

- Require pull requests before merging.
- Require approvals from at least one reviewer.
- Require branches to be up to date before merging.
- Require conversation resolution before merging.
- Block force pushes and branch deletion.
- Restrict direct pushes to `main`.

The deployment workflow is intentionally not a required pull request check. It runs after merge to `main` or manually through `workflow_dispatch`.
