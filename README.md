<p align="center">
<img src="https://github.com/JTrotta/RaspberrySharp/blob/master/RaspberrySharp/Images/Icon.png?raw=true" width="128">
</p>


# RaspberrySharp

RaspberryShap is a high performance .NET library for Raspberry Pi boards. Based on former Raspberry-Sharp (R.I.P.), it has been completely refactored. 
It provides all IO bus availbables on Pis, included Compute Model 3 and GPIO over 31. 
The implementation is based on the work by Eric Bezine <http://www.raspberry-sharp.org/author/eric-bezine/>.

## Features
* GPIO from 1 to 45 (CM3 has 45 GPIOs)
* I2C bus			      (included repeated start )
* SPI				        (beta)
* no dependacy, just one library

## Supported frameworks

* .NET Standard 2.0+
* .NET Framework 4.5.2+ (x86, x64, AnyCPU)
* Mono 5.2+

## Supported Raspberry Pi versions

* Raspberry Pi 1 model A
* Raspberry Pi 1 model A+
* Raspberry Pi 1 model B  
* Raspberry Pi 1 model B+ 
* Raspberry Pi 2 model B
* Raspberry Pi 3 model B
* Raspberry Pi Zero
* Raspberry Pi Zero W
* Raspberry Pi CM1
* Raspberry Pi CM3


## Nuget

This library is available as a nuget package: <https://www.nuget.org/packages/RaspberrySharp/>

## Contributions

If you want to contribute to this project just create a pull request. But only pull requests which are matching the code style of this library will be accepted. Before creating a pull request please have a look at the library to get an overview of the required style.




If you use this library and want to see your project here please let me know.

## MIT License

Copyright (c) 2018 Gerardo Trotta

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
