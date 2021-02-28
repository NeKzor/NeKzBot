# Admin

Note: Requires to have special permissions.

## .invites

Lists all invites of the server.

|||
|---|---|
|User Permissions|Manage Guild|
|Bot Permissions|Manage Guild|
|Lifetime|5 Minutes|
|Paginated||

## .audits <count = 10>

Lists audit logs of the server.

|||
|---|---|
|User Permissions|View Audit Log|
|Bot Permissions|View Audit Log|
|Lifetime|5 Minutes|
|Paginated||

## .pin <message_id>

Manually pins a given message. Will ask to create a new pin board for the server if it does not exist.

|||
|---|---|
|User Permissions|Manage Guild|
|Bot Permissions|Manage Webhooks|
|Lifetime|20 Seconds|
|Interactive||

## .pin.set

Configures pin board settings. Asks for the number of pins required, then for the emoji to use for pinning messages.

|||
|---|---|
|User Permissions|Manage Guild|
|Bot Permissions|Manage Webhooks|
|Lifetime|20 Seconds|
|Interactive||
