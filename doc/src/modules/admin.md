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

Configures pin board settings. Asks for the number of pins required, the emoji to use for pinning messages and then the
amount of days until a message should be ignored for pinning.

|||
|---|---|
|User Permissions|Manage Guild|
|Bot Permissions|Manage Webhooks|
|Lifetime|20 Seconds|
|Interactive||

## .pin.set.reactions <minimum_reactions>

Configures pin board setting for number of minimum reactions required for pinning messages.

|||
|---|---|
|User Permissions|Manage Guild|
|Lifetime|20 Seconds|

## .pin.set.emoji <emoji>

Configures pin board setting for the emoji to use for pinning messages.

|||
|---|---|
|User Permissions|Manage Guild|
|Lifetime|20 Seconds|

## .pin.set.days <days_until_message_expires>

Configures pin board setting for the amount of days until a message should be ignored for pinning.

|||
|---|---|
|User Permissions|Manage Guild|
|Lifetime|20 Seconds|
