# Splunk Saved Search Bulk Update Use Cases

## Purpose

Define two use cases for a C# Web API that reads and updates Splunk saved searches through a deprecated Splunk SDK. The initial version supports dry-run previews and does not write changes to Splunk.

## UC-1: Find saved searches by name or SPL regex and replace SPL text

### Goal

Find saved searches by name or by a regex that matches their SPL, then replace every regex match in the SPL with a replacement string.

### Primary actor

An authorized API client or administrator.

### Trigger

The client submits a request to preview a bulk SPL replacement.

### Preconditions

- The API can authenticate to Splunk and read the selected saved searches.
- The request uses one selection mode: saved-search names or an SPL regex.
- The regex is valid and within configured limits.

### Request inputs

- Selection mode: `Names` or `SplRegex`.
- Saved-search names, when using `Names`.
- Regex pattern and replacement string, when using `SplRegex`.
- Optional regex options, such as case sensitivity.
- Dry-run flag. In the initial version, changes are previewed and not written to Splunk.

### Main flow

1. Validate the request, selection mode, regex, replacement, and safety limits.
2. Retrieve saved searches by name, or identify saved searches whose SPL matches the regex.
3. For each selected search, replace all matches in its SPL with the replacement string.
4. Return a preview per search containing its name, match count, original SPL, proposed SPL, and whether it would change.
5. Do not write to Splunk during dry run.

### Example

```text
SPL:
index=payments provider=BankA OR provider=BankB

Regex:
provider=(BankA|BankB)

Replacement:
provider=BankX
```

The preview shows both matches replaced with `provider=BankX`.

### Alternate and error flows

- Invalid regex: reject the request before processing searches.
- No saved searches or SPL matches: return a successful no-op.
- A named search cannot be found or read: report it in the response.
- Regex execution exceeds configured limits: stop processing safely and report the reason.
- A selected search has no replacement matches: report it as unchanged.

### Rules and safeguards

- Regex matching for selection is performed against saved-search SPL.
- Replacement is global within each selected SPL, like `sed` replacing all matches.
- Treat the regex and replacement string as provided. If the replacement contains capture references such as `$1`, use the corresponding regex capture group; if it contains no references, insert it as written.
- Bound regex input size and execution time.
- Return per-search previews so the caller can review proposed changes.

### Acceptance outcome

The caller can preview a global SPL replacement for searches selected by name or by a regex that matches their SPL.

## UC-2: Find and replace saved-search action settings using JSON objects

### Goal

Find action settings objects in selected saved searches that match a supplied JSON object, then replace every matching object with the supplied replacement object.

### Primary actor

An authorized API client or administrator.

### Trigger

The client submits a request to preview a bulk action-settings replacement.

### Preconditions

- The API can authenticate to Splunk and read the selected saved searches and their action settings.
- The request identifies the saved searches or scope to inspect.
- The match and replacement objects are valid JSON objects with compatible structure.

### Request inputs

- Saved-search selection scope, such as explicit names or all searches in a defined namespace.
- JSON object to match against action settings.
- Replacement JSON object.
- Dry-run flag. In the initial version, changes are previewed and not written to Splunk.

### Matching and replacement rules

- `null` fields in the match object are ignored when checking for a match.
- Replace the entire action settings object for every match; do not update only the fields used to match.
- `null` fields in the replacement object are ignored, leaving their corresponding settings unchanged.
- Apply non-null replacement values according to the same object structure as the existing action settings.
- Replace all matching objects across all selected saved searches, including multiple matches in one search.

### Main flow

1. Validate the scope and both JSON objects.
2. Retrieve the selected saved searches and inspect their action settings.
3. Compare each action settings object with the match object, ignoring null fields in the match.
4. For every matching object, construct the proposed replacement, leaving settings corresponding to null replacement fields unchanged.
5. Return a preview per saved search with the current and proposed action settings and the number of objects that match.
6. Do not write to Splunk during dry run.

### Example

Match object:

```json
{
  "action.webhook": "1",
  "action.webhook.param.url": "https://old.example/hooks",
  "action.email": null
}
```

Replacement object:

```json
{
  "action.webhook": "1",
  "action.webhook.param.url": "https://new.example/hooks",
  "action.email": null
}
```

The match ignores `action.email` because its match value is null. Each matching action settings object is proposed for replacement; the null `action.email` replacement value leaves that setting unchanged.

### Alternate and error flows

- Invalid JSON or incompatible structures: reject before processing searches.
- No action settings objects match: return a successful no-op.
- A saved search cannot be read or its settings cannot be interpreted: report it and continue processing other searches.
- The target webhook URL is invalid or not permitted by Splunk: report the issue in the preview or validation response. Splunk Enterprise 9.0 and later checks webhook destinations against its allow list.

### Rules and safeguards

- Return a per-search preview, including current and proposed values and match counts.
- Do not write to Splunk during dry run.
- Report partial outcomes per search instead of hiding other results when one search fails.
- Keep the target URL within the organization’s and Splunk’s webhook allow-list policy.

### Acceptance outcome

The caller can preview which action settings objects match a JSON object and see the proposed replacements, with null fields ignored according to the stated rules.

## Shared implementation notes

- Keep the deprecated Splunk SDK behind an adapter so SDK-specific communication can be replaced independently of the use-case logic.
- The saved-search endpoint is used to read and edit a saved search and its alert settings. The `alerts/alert_actions` endpoint describes available alert actions; it is not a replacement for editing each saved search’s action configuration.
- The initial version is dry-run only. A later version can add an explicit apply mode after preview and review.
