# OpenBullet
OpenBullet is a webtesting suite that allows to perform requests towards a target webapp and offers a lot of tools to work with the results. This software can be used for **scraping** and **parsing data**, automated **pentesting**, unit testing through **selenium** and much more.

**IMPORTANT!** Performing (D)DoS attacks or credential stuffing on sites you do not own (or you do not have permission to test) is **illegal!** The developer will not be held responsible for improper use of this software.

![Runner](https://i.imgur.com/vb8OUfr.jpg)

[Here](https://openbullet.github.io) you can find the complete documentation for **usage**, **config making** and the **RuriLib API**.

Found a bug? ![Create an issue!](https://help.github.com/en/articles/creating-an-issue)

# How to compile
1. Clone the [official Extreme.Net repository](https://github.com/nickolasKrayn/Extreme.Net).
2. **Compile** it and obtain the **Extreme.Net.dll** file. You will find it inside the folder Extreme.Net/bin/Debug.
3. **Clone this repository** and open it in Visual Studio.
4. Right click on each of the 3 projects and from the menu select **Add > Reference**.
5. Add the previously compiled **Extreme.Net.dll** file and click OK.
6. Switch to **Release** mode for a much cleaner output.
7. **Compile** the solution (Visual Studio will fetch all the missing nuget packages).
8. You can find the compiled projects under OpenBullet/bin/Release and OpenBulletCLI/bin/Release.

# License
This software is licensed under the MIT License.

# Donate
If you like this software, consider making a donation to the developer. Thank you!
- BTC: **39yMkox6pP8tnSC7rZ5EM4nUUHgPbg1fKM**

# Credits
I want to thank all the community for their inputs that shaped OpenBullet into what it is now, and my gratitude goes towards **demiurgo** and **meinname**, who spent a lot of time helping me test and debug the Beta builds.
