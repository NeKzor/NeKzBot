# Permissions

#### Default:

Minimal|Description
---|---
Read Messages|-
Send Messages|-

---
#### Not so important but can be useful:

Special|Description
---|---
Embed Links|-
Change Nickname¹|Change bot's nickname by [command](https://github.com/NeKzor/NeKzBot/blob/master/NeKzBot/Modules/Private/Owner/Admin.cs#165).
Manage Messages²|Pin message by [command](https://github.com/NeKzor/NeKzBot/blob/master/NeKzBot/Modules/Private/Owner/Admin.cs#181) and mass delete messages by [command](https://github.com/NeKzor/NeKzBot/blob/master/NeKzBot/Modules/Private/Owner/Admin.cs#214).
Add Reactions²|Add reaction to message by [command](https://github.com/NeKzor/NeKzBot/blob/master/NeKzBot/Modules/Private/Owner/Admin.cs#131).
Create Instant Invite²|Create a channel invite by [command](https://github.com/NeKzor/NeKzBot/blob/master/NeKzBot/Modules/Public/Others/Rest.cs#L28).

---
#### Not relevant until your server is on the VIP list:

VIP Only|Description
---|---
Attach Files|Send pictures and resources.
Manage Webhooks³|Notification feed with Discord webhooks by a custom [service](https://github.com/NeKzor/NeKzBot/blob/master/NeKzBot/Modules/Public/Vip/Service.cs#25).
Connect³|Connect bot to voice channel by [command](https://github.com/NeKzor/NeKzBot/blob/master/NeKzBot/Modules/Public/Vip/Sound.cs#74).
Speak|Play sound in voice channel.

---
##### Notes:

¹ Requires the user to have the manage nicknames permission.

² Requires the user to have the same permission.

³ Requires the user to have the admin permission.

---
##### For bot developers:

Very useful calculation tool by [FiniteReality](https://finitereality.github.io/permissions).