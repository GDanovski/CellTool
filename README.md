# CellTool

CellTool is a stand-alone open source software with a Graphical User Interface for image analysis, optimized for measurements of time-lapse microscopy images. Complex image analysis workflows often require multiple software packages and constant feedback between data processing and results presentation. To solve this problem and to streamline the image analysis process, we combined data management, image processing, mathematical modeling and graphical presentation of data in a single package.</br>

</t>Software website: [https://dnarepair.bas.bg/software/CellTool/](https://dnarepair.bas.bg/software/CellTool/)</br>
</t>Publication: [https://www.mdpi.com/1422-0067/24/23/16784](https://www.mdpi.com/1422-0067/24/23/16784)</br>
</t>Source code: [https://github.com/GDanovski/CellTool](https://github.com/GDanovski/CellTool)</br>
</t>Video overview: [https://www.youtube.com/watch?v=omkshInRKKE&feature=youtu.be&t=641](https://www.youtube.com/watch?v=omkshInRKKE&feature=youtu.be&t=641)</br>

![screenshot](https://github.com/GDanovski/CellTool/blob/master/docs/CellToolScreen.png)

## How to install the application

CellTool is Windows Forms application written in c# for .NET Framework 4.5. It is available as [ClickOnce](https://en.wikipedia.org/wiki/ClickOnce) application and can be downloaded and installed from our [website](https://dnarepair.bas.bg/software/CellTool/downloads.html). Precompiled version of the program can be found in the “[build](https://github.com/GDanovski/CellTool/tree/master/build)” folder of the project.

## How to build the application from source code

1.	Download and install [Microsoft Visual Studio](https://visualstudio.microsoft.com/)
2.	Clone or download [CellTool repository](https://github.com/GDanovski/CellTool)
3.	Open the project file “Cell Tool 3.sln” in Microsoft Visual Studio
4.	Add reference to [CellToolDK library](https://github.com/GDanovski/CellToolDK) located in the “References” folder of the project 
5.	Build and start the application

## Dependencies
-	[Accord.NET](http://accord-framework.net/)
-	[BioFormats.Net](https://github.com/GDanovski/BioFormats.Net)
-	[LibTiff.Net](https://bitmiracle.com/libtiff/)
-	[Cloo.clSharp](https://www.nuget.org/packages/Cloo.clSharp/)
-	[IKVM](http://www.ikvm.net/)
-	[MathNet.Numerics](https://numerics.mathdotnet.com/)
-	[Microsoft.Solver.Foundation](https://www.nuget.org/packages/Microsoft.Solver.Foundation)
-	[NCalc](https://github.com/sheetsync/NCalc)
-	[OpenTK](https://github.com/opentk/opentk)

## How to test the application
Detailed [user guide](https://github.com/GDanovski/CellTool/blob/master/docs/CellTool_UserGuide.pdf) and examples of usage are available in the repository “docs” folder. [Test images files](https://github.com/GDanovski/CellTool/tree/master/test) can be found in the repository “test” folder. Protocol for analysis of this images is available [here](https://dnarepair.bas.bg/software/CellTool/tutorials.html#pf1f). Video overview of CellTool analysis and mathematical modelling software is available at URL: [https://www.youtube.com/watch?v=omkshInRKKE&feature=youtu.be&t=641](https://www.youtube.com/watch?v=omkshInRKKE&feature=youtu.be&t=641)
