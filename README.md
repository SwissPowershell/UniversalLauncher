# YWrapper / Universal Launcher

## Note

*This project is free of use but will not be supported in any way.*

*This project was one of my first VB project as a packager*
*i was not a developper at this time*
*(and i'm still not)*

*I am actually a decent powershell scripter*
*but this exe saved my life more than once so I wanted to share*

## Synopsys

This project aim to provide a "Launcher" (launcher.exe) that allow you to launch any exe (launched exe)

It will generate an exe file (YWrapper.exe) and a config file (YWrapper.exe.config)

You can rename the exe as soon you also rename the config file accordingly

By changing value in the config file "MySettings" you can change the way the launcher will act.

### This project has been first created in 2010 has an helper

- To shorten java command line

- To hide process (mostly powershell or any command process like cmd.exe)

**!! DO NOT USE THIS SOLUTION FOR HACKING OR OBFUSCATION !!**

## Structure of the config file (YWrapper.My.MySettings)

Each of the settings can be altered by changing the content of the \<Value>*valuetochange*\</value>

## **Wait**

**Type :** Boolean

**Valid Values :** true | false

This setting determine if the launcher will remain "running" until the "launched executable" is closed.

- With Wait set to true the "launcher.exe" will remain running until the "launched exe" is stopped.
- With Wait set to false the "launcher.exe" will close as soon the "launched exe" is started

## **Hidden**

**Type :** Boolean

**Valid Values :** true | false

This setting determine the style in wich the launched process should start.

- With Hidden set to true the "launched exe" will be started "hidden"
- With Hidden set to false the "launched exe" will be started "normally"

## **Verbose**

**Type :** Boolean

**Valid Values :** true | false

This setting is mostly for "debuging" it will show a debug window with a summary of what has been received and what will be finally executed.

Should be set to "false" outside of debugging

## **ExeName**

**Type :** String

**Valid Values :** Executable name | Executable fullpath

This setting determine the process that the launcher will execute.

remind that the launcher will work like anything "typed" in the windows "run" or in a command line interpreter (CMD)

- If you put a full command line it has to exist or the launcher will return a "file does not exist" error
- if you put a single exe it will rely on "Path" and "AppPath" to start the given exe

## **ParamChar**

**Type :** Char

**Valid Values :** Any Char (Usually "%")

This setting allow you to define the way you will identify your parameter in the Constructor (see bellow) there is no real reason to change this.

## **Constructor**

**Type :** String

**Valid Values :**

This setting allow you to append string or "Reorder" the received parameter that will be passed to the "Launched exe".

It can be empty if you did not want to append or reorder the command line.

To Reorder it use the *ParamChar* setting and number from 1 to n to identify parameter positions.

If you did not put a *"ParamChar"* in the *"Constructor** it will not reorder the arguments.

> ## Example 1 : Reordering parameter (inverse first and second param)
>
> **Content of the config file parameter "Constructor"**
>
> \<setting name="Constructor" serializeAs="String">
>
> \<value>%2 %1\</value>
>
> \</setting>
>
> \<setting name="ParamChar" serializeAs="String">
>
> \<value>%\</value>
>
> \</setting>
>
> **Command line been executed**
>
> \> YWrapper.exe MyFirstParam MySecondParam
>
> **Command line send to the final "Executed" process**
>
> \> TargetExe.exe MySecondParam MyFirstParam

You can reorder as many parameter you want if your **constructor** as more parameter than the one received the missing parameter will not be shown

> ## Example 2 : Append text to parameter (append -MyDefaultParameter)
>
> **Content of the config file parameter "Constructor"**
>
> \<setting name="Constructor" serializeAs="String">
>
> \<value>-MyDefaultParameter\</value>
>
> \</setting>
>
> **Command line been executed**
>
> YWrapper.exe MyFirstParam MySecondParam
>
> **Command line send to the final "Executed" process**
>
> TargetExe.exe -MyDefaultParameter MyFirstParam MySecondParam
>

Note : there is **no easy way** to append the parameter in the end of the command line outside knowing the exact amount of parameter that will be sent to the launcher.
