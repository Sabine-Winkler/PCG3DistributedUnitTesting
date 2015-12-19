@echo off

rem set "COMPILE_MODE=release" 
set "COMPILE_MODE=debug"

set "PROGRAM_CLIENT=.\PCG3.Client.GUI\bin\%COMPILE_MODE%\PCG3.Client.GUI.exe"
set "PROGRAM_SERVER=.\PCG3.Server.Console\bin\%COMPILE_MODE%\PCG3.Server.exe"
set "ASSEMBLY=.\PCG3.TestUnitTests\bin\%COMPILE_MODE%\PCG3.TestUnitTests.dll"
rem === server1
set "SERVER1_PORT=9000"
set "SERVER1_PARALLEL_DEGREE=4"
set "SERVER1=localhost:%SERVER1_PORT%"
rem === server2
set "SERVER2_PORT=9001"
set "SERVER2_PARALLEL_DEGREE=2"
set "SERVER2=localhost:%SERVER2_PORT%"

rem === start servers (server1, server2)
echo "Start server1 ..."
start %PROGRAM_SERVER% %SERVER1_PARALLEL_DEGREE% %SERVER1_PORT%
echo "Start server2 ..."
start %PROGRAM_SERVER% %SERVER2_PARALLEL_DEGREE% %SERVER2_PORT%

rem === start client
echo "Start client ..."
start %PROGRAM_CLIENT% %ASSEMBLY% %SERVER1% %SERVER2%