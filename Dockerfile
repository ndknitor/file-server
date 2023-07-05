# Base image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
# Set the working directory
WORKDIR /app
# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore
# Copy the remaining files and build the project
COPY . ./
RUN dotnet publish -c Release -o out
# Final image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
# Set the working directory
WORKDIR /app
# Copy the published output from the build image
COPY --from=build /app/out .
# Expose the desired port (replace 80 with your port number if needed)
EXPOSE 80
# Mount the wwwroot folder to a physical drive
VOLUME /app/wwwroot
# Set the entry point for the application
ENTRYPOINT ["dotnet", "FileServer.dll"]


#build the image
#docker build -t file-server .

#run the image
#docker run -d -p 5000:80 -v /path/to/host/folder:/app/wwwroot file-server