If you have any problems getting the samples to build or run, here are some general tips.

I recommend trying this first

- Update Visual Studio 2019 to the latest version
- Use the Visual Studio installer to install all Xamarin components
- Make sure Visual Studio is closed

1. Pull latest on `develop` (outside of Visual Studio - Leave it closed until the last step)
2. [Clear NuGet cache](https://stackoverflow.com/a/34935038/1878141)
3. Make sure you have no uncommitted changes.
4. Do a [Git clean](https://git-scm.com/docs/git-clean) on the repo. Make sure you remove all untracked files.

> git clean -x -f -d

5. Open Visual Studio and open the appropriate solution in the src folder.
6. Make sure you are in _Debug_ build mode.
7. Restore NuGet packages. [Make sure that all packages are restored before trying to build](https://docs.microsoft.com/en-us/nuget/consume-packages/package-restore-troubleshooting).
8. Try building in _Debug_ again
9. Repeat from step 1 with [develop branch](https://github.com/MelbourneDeveloper/Device.Net/tree/develop). _Note: from time to time, the develop branch will be slightly out from the master branch. It's worth trying both._

- If you still get an error, please document it up with as much detail, and put a question on [Stack Overflow](https://stackoverflow.com/). Tag it with "usb, hid", mention "Device.Net" and any tags for your OS, etc.
- If you do put a question on Stack Overflow, please reference it in an issue on this repo.