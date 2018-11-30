FROM microsoft/dotnet:sdk AS build-env
RUN mkdir /app
WORKDIR /app
# Copy csproj and restore as distinct layers
COPY *.csproj ./
# Copy everything else and build
COPY . ./
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]
EXPOSE 5003 
CMD ["dotnet","run"]
# Build runtime image
# FROM microsoft/dotnet:aspnetcore-runtime
# WORKDIR /app
# COPY --from=build-env /app/out .
# ENTRYPOINT ["dotnet", "module_sme.dll"]
