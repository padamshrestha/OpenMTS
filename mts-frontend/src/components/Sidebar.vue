<template>
  <aside id="sidebar" :class="{ collapsed: menuCollapsed }">
    <div id="menu">
      <MenuHeader :label="$t('general.material')" :collapsed="menuCollapsed" />
      <MenuButton :label="$t('general.sites')" icon="el-icon-office-building" to="/private/sites" />
      <MenuButton :label="$t('general.inventory')" icon="el-icon-box" to="/private/inventory" />
      <MenuButton :label="$t('general.materials')" icon="el-icon-document" to="/private/materials" />
      <MenuButton :label="$t('general.plastics')" icon="el-icon-notebook-2" to="/private/plastics" />
      <MenuButton :label="$t('general.trace')" icon="el-icon-s-promotion" to="/private/trace" />
      <div v-if="userIsAuthenticated && userIsAdmin">
        <MenuHeader :label="$t('general.administration')" :collapsed="menuCollapsed" />
        <MenuButton :label="$t('general.configuration')" icon="el-icon-s-operation" to="/private/config" />
        <MenuButton :label="$t('general.users')" icon="el-icon-user-solid" to="/private/users" />
        <MenuButton :label="$t('general.apiKeys')" icon="el-icon-key" to="/private/keys" />
        <MenuButton :label="$t('general.locations')" icon="el-icon-office-building" to="/private/locations" />
      </div>
    </div>
  </aside>
</template>

<script>
import MenuHeader from './Sidebar/MenuHeader.vue';
import MenuButton from './Sidebar/MenuButton.vue';

export default {
  name: 'sidebar',
  components: { MenuHeader, MenuButton },
  data() {
    return {
      userName: localStorage.getItem('name'),
      userRole: localStorage.getItem('role'),
    };
  },
  computed: {
    menuCollapsed() {
      return this.$store.state.ui.menuCollapsed;
    },
    userIsAuthenticated() {
      return this.$store.state.token !== null;
    },
    userIsAdmin() {
      return this.userRole === '0';
    },
  },
};
</script>

<style lang="scss" scoped>
@import '../theme/colors.scss';

#sidebar {
  position: fixed;
  top: 61px;
  left: 0;
  min-height: 100%;
  width: 229px;
  background-color: $color-menu-primary;
  border-right: 1px solid black;
  color: $color-menu-light-text;
  transition: all 0.5s ease-in-out;
  &.collapsed {
    width: 47px;
  }
  z-index: 2;
}

#menu {
  height: auto;
  text-align: left;
  font-size: 16px;
}

.collapse-enter-active,
.collapse-leave-active {
  transition: all 0.5s ease-in-out;
  overflow: hidden;
}
.collapse-enter,
.collapse-leave-to {
  width: 0;
}
</style>
