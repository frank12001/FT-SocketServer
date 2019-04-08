# Use an official Python runtime as a parent image
FROM microsoft/dotnet:aspnetcore-runtime

# Set the working directory to /app
WORKDIR /app

# Copy the current directory contents into the container at /app
COPY . /app

# Make port 5000 available to the world outside this container
EXPOSE 30100:30100/udp

# Run app.py when the container launches
CMD ["dotnet", "SocketServerDemo1.dll"]