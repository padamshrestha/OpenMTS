<template>
  <div id="private">
    <Sidebar />
    <Navbar />
    <div id="content" :class="{ collapsed: menuCollapsed }">
      <router-view></router-view>
    </div>
  </div>
</template>

<script>
import Navbar from '@/components/Navbar.vue';
import Sidebar from '@/components/Sidebar.vue';

export default {
  name: 'private',
  components: { Navbar, Sidebar },
  computed: {
    menuCollapsed() {
      return this.$store.state.ui.menuCollapsed;
    },
  },
  methods: {
    fitToScreen: function() {
      if (window.innerWidth <= 800) {
        this.$store.commit('collapseSidebar');
      } else {
        this.$store.commit('expandSidebar');
      }
    },
  },
  created: function() {
    window.addEventListener('resize', this.fitToScreen);
    this.fitToScreen();
  },
  destroyed() {
    window.removeEventListener('resize', this.fitToScreen);
  },
};
</script>

<style lang="scss">
@import '../theme/colors.scss';

.page-small {
  max-width: 800px;
  margin: auto;
}

.page-medium {
  max-width: 1000px;
  margin: auto;
}

.page-large {
  max-width: 1200px;
  margin: auto;
}

#content {
  margin-top: 61px;
  margin-left: 230px;
  padding: 0px 16px;
  text-align: center;
  transition: all 0.5s ease-in-out;
  &.collapsed {
    margin-left: 48px;
  }
  & > div {
    text-align: left;
  }
}

.content-row {
  padding-top: 16px;
  overflow: auto;
  .left.content-title {
    padding-top: 4px;
  }
  .left.content-subtitle {
    padding-top: 8px;
  }
}

.content-section {
  padding-top: 8px;
  padding-bottom: 8px;
  border-bottom: 1px solid lightgray;
  & .content-row {
    padding-top: 8px;
  }
}

.content-section:last-child {
  border: none;
}

.content-row-nopad {
  overflow: auto;
}

.content-row-inputs {
  overflow: hidden;
}

.content-title {
  font-size: 18px;
  font-weight: bold;
}

.content-subtitle {
  font-size: 16px;
  font-weight: 600;
}

.flex {
  display: flex;
  justify-content: space-between;
  flex-wrap: wrap;
}

.grow {
  flex-grow: 1;
}

.left {
  float: left;
}

.right {
  float: right;
  button {
    margin-left: 8px;
  }
}

table td {
  cursor: pointer;
}

.current-row > td {
  background-color: $color-info !important;
  color: white !important;
}

.input-button {
  color: $color-primary !important;
}

.el-select-dropdown {
  text-align: left;
}
</style>
