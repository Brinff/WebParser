FROM dpage/pgadmin4:latest

USER root

COPY servers.json /pgadmin4/servers.json
COPY pgpass /pgpass

RUN chmod 600 /pgpass && chown 5050:0 /pgpass

USER pgadmin