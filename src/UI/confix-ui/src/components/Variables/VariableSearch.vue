<template>
  <v-row>
    <v-col>
      <v-text-field
        clearable
        placeholder="Search"
        prepend-icon="mdi-magnify"
        append-outer-icon="mdi-plus"
        @click:append-outer="onClickAddVariable"
        v-model="searchText"
      ></v-text-field>

      <v-list rounded dense class="mt-0">
        <v-list-item-group v-model="selectedVar" color="primary">
          <v-list-item
            v-for="variable in variables"
            :key="variable.id"
            selectable
            @click="onSelectVariable(variable)"
          >
            <v-list-item-content>
              <v-list-item-title v-text="variable.name"></v-list-item-title>
            </v-list-item-content>
            <v-list-item-action v-if="variable.isSecret">
              <v-icon color="yellow lighten-2">mdi-lock-outline</v-icon>
            </v-list-item-action>
          </v-list-item>
        </v-list-item-group>
      </v-list>
    </v-col>
  </v-row>
</template>

<script>
import { mapActions, mapState } from "vuex";
export default {
  created() {
    this.loadVariables();
  },
  data() {
    return {
      selectedVar: null,
      searchText: "",
    };
  },
  computed: {
    ...mapState("vars", ["vars"]),
    variables: function () {
      if (this.searchText) {
        const regex = new RegExp(`.*${this.searchText}.*`, "i");
        return this.vars.filter((x) => regex.test(x.name));
      }
      return this.vars;
    },
  },
  methods: {
    ...mapActions("shell", ["openTab"]),
    ...mapActions("vars", ["loadVariables"]),

    onSelectVariable: function (variable) {
      this.openTab({
        type: "VARIABLE",
        title: variable.name,
        id: variable.id,
        item: {
          variable,
        },
      });
    },
    onClickAddVariable: function () {
      this.openTab({
        type: "VARIABLE_ADD",
        title: "New Variable",
        id: "VAR_NEW",
        item: null,
      });
    },
  },
};
</script>

<style>
</style>