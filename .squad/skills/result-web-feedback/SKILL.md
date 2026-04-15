---
name: "result-web-feedback"
description: "Apply Result<T>-based feedback patterns in Razor Pages and HTMX flows."
domain: "web-ui"
confidence: "high"
source: "earned"
---

## Context
Use this when a Web-layer flow moves from sentinel values to `Result`/`Result<T>`. It covers Razor Page handlers, redirect feedback, and HTMX partial re-renders.

## Patterns
- For admin redirect flows, send failure text through `TempData["ErrorMessage"]` so `_ToastMessages` renders it after the redirect.
- For same-page create/edit posts, add `ModelState` errors and return `Page()` so validation summaries show the failure.
- For HTMX partials, keep the server-populated response model (`Model.NewExpertiseResponse`) when re-rendering instead of creating a new one.
- For lookup/filter HTMX handlers, return small server-rendered HTML error fragments when a `Result` fails.

## Examples
- `Areas\Admin\Pages\Catalog\Expertises\Categories\Index.cshtml.cs`
- `Areas\Identity\Pages\Account\Register.cshtml.cs`
- `Pages\Profile\Edit.cshtml.cs`
- `Pages\Speakers\Index.cshtml.cs`
