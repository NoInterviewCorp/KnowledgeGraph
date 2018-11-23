FROM microsoft/dotnet:sdk AS build-env
COPY . /app
WORKDIR /app
# Copy csproj and restore as distinct layers
# COPY *.csproj ./
# Copy everything else and build
# COPY . ./
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"] 
EXPOSE 5000
# Build runtime image
# FROM microsoft/dotnet:aspnetcore-runtime
# WORKDIR /app
# COPY --from=build-env /app/out .
# ENTRYPOINT ["dotnet", "module_sme.dll"]
CMD ["dotnet","run"]
