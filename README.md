_Fiz_ is a simple media viewer for picture, video, and web page media to display content during a video conference.

## FEATURES
* Display picture media (bmp, gif, jpeg, ico, png, tiff, webp)
* Display video media (avi, mov, mp4, mpeg, wmv)
* Display web pages
* Display windowed or fullscreen on a secondary monitor
* Three modes: picture + video list, single video, web page
* Can start video at any location in single video mode
* Pan and zoom picture media
* Pan and zoom web pages

## REQUIREMENTS
* Target OS version: Windows 10.0.19041.0
* Minimal OS version: Windows 10.0.17763.0

## BUILD AND DEPLOY STEPS
* Download and install [Visual Studio](https://visualstudio.microsoft.com/).
* Include the __.NET desktop development__ workload in the installation.
* Publish the __win-x64__ profile.
* Copy `Fiz.exe` from the `ui/bin/publish` folder to the client computer.

## Media List Mode

| ![media list](/assets/readme-main-window-media-list.png) |
| - |

## Single Video Mode

| ![single video mode](/assets/readme-main-window-single-video-mode.png) |
| - |
