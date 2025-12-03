#!/bin/sh
# Simple health check script for the API
# Just verifies that the dotnet process is running

ps aux | grep -v grep | grep dotnet > /dev/null
exit $?
