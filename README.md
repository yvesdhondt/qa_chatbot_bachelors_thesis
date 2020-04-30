# P&O Entrepreneurship - Team A - Virtual Company Assistant (code) a.k.a Cluster
## About this project
This is the code base repository of our bachelor's thesis project. 

## Modules
The chatbot and its helper services depend on (and therefore all communicate with) the Cluster Connector Server, to which a connection is established using the `cluster-connector` (Python connector) or `ClusterClient` (C#/.NET Core connector) libraries. All connector related code can be found in the [Cluster Connector repository](https://github.com/heckej/P-O-Entrepreneurship-Team-A-ClusterConnector). Another part of Cluster is the [Cluster Moderator](https://github.com/yvesdhondt/P-O-Entrepreneurship-Moderator/), a tool to be used by someone who moderates the questions and answers provided to the chatbot by the user.

## Documentation
Documentation can be found at [Clusterdocs](https://heckej.github.io/P-O-Entrepreneurship-Team-A-ClusterConnector/).
