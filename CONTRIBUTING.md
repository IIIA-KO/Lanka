# Contributing to Lanka

Thank you for your interest in contributing to Lanka! 
This document outlines our workflow and naming conventions to help keep our repository organized and our development process consistent.

---

## Table of Contents

- [Reporting Issues](#reporting-issues)
- [Branch Naming Conventions](#branch-naming-conventions)
- [Commit Message Conventions](#commit-message-conventions)
- [Pull Request Guidelines](#pull-request-guidelines)
- [Issue Numbering](#issue-numbering)
- [General Guidelines](#general-guidelines)

---

## Reporting Issues

When you encounter a bug, have a feature request, or want to propose an enhancement, please open an issue using one of. Ensure that:
- The title is clear and descriptive.
- You fill out all requested details in the template.
- You add appropriate labels (e.g., `type: feature`, `type: bug`, `priority: high`, `module: user-auth`, etc.) to help us categorize the issue.

---

## Branch Naming Conventions

Please use the following format for branch names:
```
<category>/<issue-number>-<short-description>
```

**Guidelines:**
- **Category Prefix:** Use one of the following:
  - `feature/` – for new functionality.
  - `bugfix/` – for fixes.
  - `enhancement/` – for improvements.
  - `chore/` – for maintenance, refactoring, or non-feature tasks.
- **Issue Number:** Use the GitHub issue number assigned automatically (e.g., `123`). Do **not** add any extra words or padding.
- **Short Description:** Provide a brief, hyphenated summary of the work.

**Examples:**
- `feature/123-add-login-endpoint`
- `bugfix/124-fix-token-refresh`
- `enhancement/125-improve-campaign-ui`
- `chore/126-update-dependencies`

---

## Commit Message Conventions

Please follow this commit message format to ensure clarity and traceability:
```
#<issue-number>: <Short summary of the commit>
```


**Guidelines:**
- **Issue Number:** Begin with the issue number prefixed by a `#` (e.g., `#123`).
- **Short Summary:** Write a concise summary in the imperative mood (e.g., “Add login endpoint”, “Fix token refresh bug”).
- **Detailed Description (Optional):** After the summary line, add a blank line and then a detailed description if needed.

**Examples:**
- `#123: Add login endpoint with JWT support`
- `#124: Fix token refresh error in middleware`
- `#125: Update campaign UI for improved usability`
- `#126: Refactor authentication middleware for clarity`

---

## Pull Request Guidelines

1. **Creating a Pull Request (PR):**
   - Ensure your branch is up-to-date with the latest changes from the main branch.
   - Open a PR against the main branch.
   - Title your PR with the issue number and a short description, e.g., `#123: Add login endpoint`.

2. **PR Description:**
   - Clearly describe the changes made.
   - Reference the corresponding issue(s) (e.g., “Fixes #123”).
   - Include screenshots or additional context if applicable.

3. **Code Review:**
   - All PRs must be reviewed by at least one other contributor.
   - Address review comments promptly.
   - Once approved, your PR will be merged and the related issue will be closed.

---

## Issue Numbering

- GitHub automatically assigns each new issue a sequential number.
- Use this issue number directly in branch names and commit messages.
- **Do not add any extra prefixes or zero-padding.**  
  For example, if an issue is assigned **123**, reference it simply as `123` (e.g., `feature/123-add-something` or commit message `#123: Fix some bug`).

---

## General Guidelines

- **Coding Standards:**  
  Follow the project's coding style and best practices.
  
- **Documentation:**  
  Update documentation as necessary when you make changes.
  
- **Testing:**  
  Write tests for new features or bug fixes to ensure the stability of the project.
  
- **Communication:**  
  Use designated channels (e.g., GitHub Discussions) for questions or clarifications.

- **Consistency:**  
  Adhere to these conventions to maintain a clean, organized repository. This makes it easier for everyone to trace changes and understand the project history.

---

Thank you for contributing to Lanka! Your efforts help make this project better for everyone. If you have any questions or suggestions, please feel free to reach out.

Happy coding!
