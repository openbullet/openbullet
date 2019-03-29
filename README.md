# OpenBullet
OpenBullet is a webtesting suite that allows to perform requests towards a target webapp and offers a lot of tools to work with the results. This software can be used for **scraping** and **parsing data**, automated **pentesting**, unit testing through **selenium** and much more.

![Runner](https://i.imgur.com/vb8OUfr.jpg)

[Here](https://openbullet.github.io) you can find the complete documentation for **usage**, **config making** and the **RuriLib API**.

# How to compile
1. Clone the [official Extreme.Net repository](https://github.com/nickolasKrayn/Extreme.Net).
2. Compile it and obtain the Extreme.Net.dll file. You will find it inside the folder Extreme.Net/bin/Debug.
3. Clone this repository and open it in Visual Studio.
4. Wait until Visual Studio fetches all the required nuget packages.
5. Right click on each of the 3 projects and from the menu select Add > Reference.
6. Add the previously compiled Extreme.Net.dll file and click OK.
7. Switch to Release mode for a much cleaner output.
8. Compile the solution.
9. You can find the compiled projects under OpenBullet/bin/Release and OpenBulletCLI/bin/Release.

# License
This software is licensed under the MIT License.

# Credits
I want to thank all the community for their inputs that shaped OpenBullet into what it is now, and my gratitude goes towards **demiurgo** and **meinname**, who spent a lot of time helping me test and debug the builds prior to the open source release.
