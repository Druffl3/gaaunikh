# AGENTS.md

## Repository Workflow

- Do not create or use git worktrees in this repository.
- Do not run `git worktree add`, `git worktree remove`, or similar worktree commands.
- Create and use regular git branches directly in the current repository workspace.
- When starting scoped work, create a branch from the current branch and continue in-place.

## Branch Guidance

- Prefer small, task-focused branches.
- Keep commits scoped to the active task.
- Do not discard or overwrite unrelated local changes unless explicitly requested.

## Local Development Notes

- The application uses Docker Compose for local supporting services.
- PostgreSQL is exposed on host port `5433`.
- Adminer is exposed on host port `8081`.
