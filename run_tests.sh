#!/bin/bash
cd /home/daan-acohen/dotnetPlayGround/Gravity
echo "Building solution..."
dotnet build Gravity.Test/Gravity.Test.csproj
echo "Build completed with exit code: $?"
echo ""
echo "Running tests..."
dotnet test Gravity.Test/Gravity.Test.csproj --logger "console;verbosity=detailed"
echo "Tests completed with exit code: $?"

