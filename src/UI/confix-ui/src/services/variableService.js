import apollo from "../apollo";
import QUERY_GET_ALL_VARIABLES from "../graphql/Variable/GetAll.gql";
import QUERY_GET_BY_ID from "../graphql/Variable/GetById.gql";
import MUTATION_VARIABLE_CREATE from "../graphql/Variable/Create.gql";
import MUTATION_SAVE_VALUE from "../graphql/Variable/SaveValue.gql";
import MUTATION_DELETE_VALUE from "../graphql/Variable/DeleteValue.gql";

export const getAllVariables = async () => {
    return await apollo.query({
        query: QUERY_GET_ALL_VARIABLES,
        variables: {}
    });
};

export const getById = async (id) => {
    return await apollo.query({
        query: QUERY_GET_BY_ID,
        variables: {
            id
        }
    });
};

export const createVariable = async (input) => {
    return await apollo.mutate({
        mutation: MUTATION_VARIABLE_CREATE,
        variables: {
            input
        }
    });
};

export const saveValue = async (input) => {
    return await apollo.mutate({
        mutation: MUTATION_SAVE_VALUE,
        variables: {
            input
        }
    });
};

export const deleteValue = async (input) => {
    return await apollo.mutate({
        mutation: MUTATION_DELETE_VALUE,
        variables: {
            input
        }
    });
};
