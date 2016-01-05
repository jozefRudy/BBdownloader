# BBdownloader
bloomberg downloader - windows desktop c# app

Desktop application to batch download bloomberg data for stocks.

It uses all the bloomberg optimizations (25 fields downloaded at the same time for batch of 50 stocks).

Your request is specified here: https://goo.gl/o2aU9E.
Feel free to create similar sheet and set your options in test.cfg accordingly.

It is able to download data that can be obtained with BDH and/or BDP excel functions.

I benchmarked it could download around 500 shares (60 fundamental fields with weekly history since 1990 for each stock) in around 40 minutes.

Data downloaded is in the form of csv files - each in its appropriate company directory.