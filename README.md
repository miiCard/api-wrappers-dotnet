api-wrappers-dotnet
================

.NET wrapper classes around the miiCard API and OAuth authorisation process.

##What is miiCard

miiCard lets anybody prove their identity to the same level of traceability as using a passport, driver's licence or photo ID. We then allow external web applications to ask miiCard users to share a portion of their identity information with the application through a web-accessible API.

##What is the library for?
miiCard's API is an OAuth-protected web service supporting SOAP, POX and JSON - [documentation](http://www.miicard.com/developers) is available. The library wraps the SOAP endpoint of the API, making it easier to make the required OAuth signed requests. It also provides a very simple code interface for initiating an OAuth exchange with miiCard to obtain OAuth access token and secret information for a miiCard member.

You can obtain a consumer key and secret from miiCard by contacting us on our support form, including the details listed on the developer site.

##Usage

## Obtaining an OAuth access token and secret

How you do this depends on if you're using WebForms or MVC (or some other framework). See the [.NET Wrapper Library documentation](http://www.miicard.com/developers/libraries-components/net-library) for more information.

To use the Claims and Financial API wrappers, you'll need:

* Consumer key and secret (available by request from miiCard)
* An access token and secret for a given miiCard member, obtained by pushing the member through the OAuth workflow

### Claims API wrapper
The API wrapper wraps the methods of the miiCard API and makes them easily accessible to managed code. To start, new one up:

    var apiWrapper =
        new miiCard.Consumers.Service.v1.MiiCardOAuthClaimsService
        (
            MIICARD_CONSUMER_KEY, MIICARD_CONSUMER_SECRET,
            accessToken, accessTokenSecret
        );

Then call methods on the wrapper corresponding to the miiCard Claims API methods - they are named identically. For example, to obtain the set of data a miiCard member elected to share with your application:

    var response = apiWrapper.GetClaims();
    var userProfile = response.Data;

### Financial API

Accessing the Financial API is the same, but with a different wrapper type - MiiCardOAuthFinancialService:

    var apiWrapper = 
        new miiCard.Consumers.Service.v1.MiiCardOAuthFinancialService
        (
            MIICARD_CONSUMER_KEY, MIICARD_CONSUMER_SECRET,
            accessToken, accessTokenSecret
        );

### Directory API

Unlike the Claims and Financial APIs, the Directory API is a public lookup service that requires no special OAuth steps to access:

    var apiWrapper = 
        new miiCard.Consumers.Service.v1.MiiCardDirectoryService();
    
    var match = apiWrapper.FindByTwitter("@pauloneilluk");

If you need to hash an identifier you can use the `MiiCardDirectoryService.HashIdentifier(identifier)` static function. This won't normalise the identifier, but will at least lowercase it before hashing - see the [Directory API](http://www.miicard.com/developers/directory-api) documentation for details on normalisation steps.

### Supported frameworks

The library also contains a drop-in server control for ASP.NET applications, and classes to aid in performing an OAuth authorisation workflow in MVC applications. See the [.NET library documentation](http://www.miicard.com/developers/libraries-components/net-library) on the miiCard.com website for more details.

##Test harness

The [/test folder](api-wrappers-dotnet/tree/master/test) contains a quick test harness to allow some interactive testing of the library. It may serve as a guide for how to quickly get up and running with the library but hasn't been extensively checked for correctness or security and should be considered a local diagnostic tool only. It is an MVC4 web application and takes dependencies on several DotNetOpenAuth libraries managed by NuGet.

##Documentation

Documentation is available at http://www.miicard.com/developers/libraries-components/net-library, and is intended to supplement the API documentation available on the [miiCard Developers site](http://www.miicard.com/developers).

##Dependencies

The library takes a dependency on [DotNetOpenAuth](http://www.dotnetopenauth.net), an excellent OAuth library by Andrew Arnott. It's available as a [NuGet package](https://www.nuget.org/packages/DotNetOpenAuth) or [source code on GitHub](https://github.com/DotNetOpenAuth/DotNetOpenAuth).

##Contributing
* Use GitHub issue tracking to report bugs in the library
* If you're going to submit a patch, please base it off the development branch if available
* Join the [miiCard.com developer forums](http://devforum.miicard.com) to keep up to date with the latest releases and planned changes

##Licence
Copyright (c) 2012, miiCard Limited All rights reserved.

http://opensource.org/licenses/BSD-3-Clause

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

- Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

- Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

- Neither the name of miiCard Limited nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.