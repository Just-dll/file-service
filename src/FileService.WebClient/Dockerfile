# Step 1: Build the Angular app
# Use an official Node.js image to build the Angular app
FROM node:18 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the package.json and package-lock.json to install dependencies
COPY package*.json ./

# Install Angular dependencies
RUN npm install

# Copy the rest of the Angular app's code
COPY . .

# Build the Angular app in production mode
RUN npm run build --prod

# Step 2: Serve the Angular app
# Use an official Nginx image to serve the app
FROM nginx:alpine

COPY ./nginx.conf /etc/nginx/conf.d/default.conf
# Copy the built Angular app from the build stage to the Nginx HTML directory
COPY --from=build /app/dist/file-service.web-client /usr/share/nginx/html

# Expose the default Nginx HTTP port
EXPOSE 80

# Start the Nginx server
CMD ["nginx", "-g", "daemon off;"]
