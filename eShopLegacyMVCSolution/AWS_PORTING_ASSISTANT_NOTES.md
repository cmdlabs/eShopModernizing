# AWS Porting Assistant Notes

## Setup User

setup a user - `dotnet-porting-assistant-tester`

Initially with no permissions - prevents saving.  UI suggests Admin or Power User (irresponsibly!)

Found:

https://docs.aws.amazon.com/portingassistant/latest/userguide/security-iam.html

which provides a more minimal policy.

After setting that up - permitted to save the profile.

## Visual Studio Extension

Install using visual Studio extensions and restart VS

### Initial assessment

91 Warnings - many similar to MS Portability Analyzer.

### Port

Ported successfully (includes migration to efcore by default!)

Now just build failures.

## Standalone tool




