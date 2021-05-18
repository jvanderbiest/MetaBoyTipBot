[![Build](https://github.com/jvanderbiest/MetaBoyTipBot/actions/workflows/build.yml/badge.svg)](https://github.com/jvanderbiest/MetaBoyTipBot/actions/workflows/build.yml) [![Coverage Status](https://coveralls.io/repos/github/jvanderbiest/MetaBoyTipBot/badge.svg?branch=master)](https://coveralls.io/github/jvanderbiest/MetaBoyTipBot?branch=master)

# MetaBoyTipBot
## _You don't have to tip anybody, anywhere, anything. You do so only because you want to, in appreciation for service well-rendered._
MetaBoy Tip Bot is a Telegram bot that is able to send tips between users using the MetaHash Coin (MHC) currency.

## Features
- Tip other users by giving 👍, + or !tip [amount]
- Check your current tip balance
- Top up your balance by making a MetaHash transaction to the tip wallet
- Withdraw to your own MetaHash wallet at any time
- 🎈 All for free, just like the MetaHash network

![MetaBoyTipBot](https://raw.githubusercontent.com/jvanderbiest/MetaBoyTipBot/master/metaboy.png)

## Tech
- .Net 5.0
- Azure Table Storage

## Build
Configure Telegram WebHook using ngrok
Build the project

```sh
dotnet build
```

## Test
Integration tests require a local Azure Storage Emulator

```sh
dotnet test
```

## License
MIT