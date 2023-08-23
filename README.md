# Redux

> This project was previously maintained by __Pro4Never__. The text below is his description and instructions for the project from [Elitepvpers](https://www.elitepvpers.com/forum/co2-pserver-guides-releases/2692305-redux-v2-official-5065-classic-source.html), modified to include updated tooling.

This is a fully functional, ready to play server emulating patch 5065. It will have some missing minor features and the odd minor bug but as of the 3.0 release this should be a fully working source for you to use in your own development (no guarantees that it will be pretty though)

I've been hosting this server live for a week now and in this update have added, fixed or tweaked all the things users have reported to the best of my ability. There may be new bugs introduced by my changes but as of right now I'm considering this project 'done' and will be working on future updates privately and may consider hosting a live server with it.

## Getting Started

[![Instructions](http://img.youtube.com/vi/Yb_zBSnvM2Y/0.jpg)](http://www.youtube.com/watch?v=Yb_zBSnvM2Y)

Setup is incredibly simple but you will need to ensure you have a proper version of mysql installed. Follow the steps below and you'll be running within minutes!

* Download [Mysql](https://dev.mysql.com/downloads/mysql/) or [MariaDB](https://mariadb.org/download/?t=mariadb&p=mariadb&r=11.1.2) (Recommended)
* Execute the SQL backup (using any MySQL management tool such as [MySQL Workbench](https://dev.mysql.com/downloads/workbench/))
* Create an Account in the database using your MySQL management tool
* Build and run the source in [Visual Studio 2022 Community](https://visualstudio.microsoft.com/downloads/)
* Enter the Server Information requested on first run (ip/name/db info)

## Troubleshooting

__NOTE__: Use network, hamachi or external ip. 127.0.0.1 will not work.

__NOTE__: If you bluescreen trying to run conquer, remove the tq anti virus folder and you will be fine.

## Client Setup

If you do not have a client already you will need to follow the next steps.

* Download a 5065 client
* Download ConquerLoader (Credits to nullable)
* Unrar the loader into the 5065 client. Edit IP inside LoaderSet.ini

TADA! You're ready to play.