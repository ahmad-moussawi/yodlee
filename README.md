Yodlee CSharp API
=================


## Setup

```cs

// Initial configuration
var cobrandName = "";
var cobrandLogin = "";
var cobrandPassword = "";
var userLogin = "";
var userPassword = "";

// Create a new instance from Yodlee
var yodlee = new YodleeApi(cobrandName);

// Enable debug mode in development
yodlee.Debug = true;

// Set to false in production
yodlee.IsSandbox = true;

// Login the cobrand first
await yodlee.Login(cobrandLogin, cobrandPassword);

// Login the user
await yodlee.Login(userLogin, userPassword);

```

## Get FastLink IFrame

```cs

// Get the FastLink token
var token = (await yodlee.FastLinkAccessToken()).Json();

// Generate the FastLink Iframe Url
var url = await yodlee.FastLinkUrl(token.Value, callbackUrl: "http://localhost:5000");
```

## Get Accounts

```cs
var response = await yodlee.Accounts();
var accounts = response.Json();
```

## Get Transactions

```cs
var response = await yodlee.Transactions(accountId, new DateTime(2007, 1, 1), new DateTime(2018, 1, 1));
var accounts = response.Json();
```