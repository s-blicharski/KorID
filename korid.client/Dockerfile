FROM node:20 AS build

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:alpine

# install envsubst
RUN apk add --no-cache gettext

# copy template and built files (copy full dist - safer)
COPY --from=build /app/default.conf.template /etc/nginx/conf.d/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["/bin/sh", "-c", "envsubst '$PORT $services__koridapi__https__0 $services__koridapi__http__0' < /etc/nginx/conf.d/default.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'"]