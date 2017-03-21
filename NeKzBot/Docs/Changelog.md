# Changelog

## Version 1.6.0
* Added new commands for speedrun module
* Added developer only commands for leaderboard module
* Added automatic restarter on unhandled class exception
* Changed cloud module permission to vip only
* Added a task to generate module lists automatically
* Replaced WebClients with HttpClients in fetching system
* Fixed caching system and improved its logic
* Improved internal data manager
* Lots of bug fixes

## Version 1.5.0
* Added support for playing audio streams on multiple servers
* Added VIP guild permission
* Changed permissions for some modules
* Removed unnecessary settings, variables and commands
* Added a bot command to list all available guilds
* Revised help mode commands
* Fixed Portal 2 leaderboard profile scores parser
* Fixed random number generator not returning the number zero
* Fixed entry comment not showing for Portal 2 webhooks
* Portal 2 leaderboard names and entry comments are encoded correctly now
* Added message embedding extension
* Changed message format into embeds for leaderboard and speedrun module
* Added embed for Steam workshop parser
* Fixed event exploits
* Added a second check if a user has a nickname for the leaderboard module
* Fixed Steam workshop parser
* Added command to list all available sound commands
* Added a profile comparison command for leaderboard module

## Version 1.4.1
* Fixed webhook file I/O
* Fixed data naming and spelling mistakes
* Improved help mode
* Revised command descriptions
* Added a Linux only permission
* Fixed notification bug
* Fixed the echo command
* Added a webhook cleanup command
* Fixed data deletion task
* Added FFmpeg support for Windows systems

## Version 1.4.0
* Added support for webhooks
* Added documentation
* Massive code cleanup
* Automatic Twitch channel detection with role assignment
* Added internal watches to make parallel tasks more accurate
* Fixed giveaway cache
* Send world record comment as tweet reply
* Improved server logger and separated events
* Added class management for data manager
* Added automatic role assignment when somebody has a world record
* Twitch stream preview is now a static attachment
* Added version, changelog join, invite and staticinvite commands
* Improved idinfo command
* Send remaining or missed world records and notifications
* Steam workshop item links are correctly parsed now
* Added additional symbols to RIS algorithm
* Fixed wrong encoding in fetching system
* Added application exit handler
* Update Twitter location after a leaderboard update
* Update Twitter description when going online or offline
* Disabled automatic link embedding
* Send updates to every connected server
* Changed every method to task
* Improved data deletion code
* Added tickrate and startdemos converter
* Fixed and improved many other things

## Version 1.3.0
* Sending leaderboard update to a Twitter account
* Fixed an exploit which crashes the bot
* Fixed leaderboard caching
* Exception logging
* Permissions checks
* Custom help mode
* Prefix handler
* Error handler
* Added namespaces
* Even more bug fixes
* Removed NSA code

## Version 1.2.0
* Added data debug commands
* New view image command
* Early support for other Discord servers
* Separated credentials from settings
* Custom http header for web client
* Lots of bug fixes

## Version 1.1.0
* Speedrun commands
* Notifications for Speedrun
* Notifications for Twitch
* File uploader to Dropbox
* Loading Steam workshop item images
* Channel logging
* Command permissions
* Implemented two caching systems
* Implemented data fetching system
* New commands
* Bug fixes

## Version 1.0.0
* Initial release