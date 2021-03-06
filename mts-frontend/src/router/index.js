import Vue from 'vue';
import VueRouter from 'vue-router';
Vue.use(VueRouter);

// Views - General
import Public from '../views/Public.vue';
import Private from '../views/Private.vue';
import Account from '../views/private/Account.vue';
import Dashboard from '../views/private/Dashboard.vue';

// Views - Material
import StorageSites from '../views/private/StorageSites.vue';
import Environment from '../views/private/StorageSites/Environment.vue';
import Inventory from '../views/private/Inventory.vue';
import CreateBatch from '../views/private/Inventory/CreateBatch.vue';
import EditBatch from '../views/private/Inventory/EditBatch.vue';
import TransactionLog from '../views/private/Inventory/TransactionLog.vue';
import Materials from '../views/private/Materials.vue';
import CreateMaterial from '../views/private/Materials/CreateMaterial.vue';
import EditMaterial from '../views/private/Materials/EditMaterial.vue';
import MaterialDetails from '../views/private/Materials/MaterialDetails.vue';
import Plastics from '../views/private/Plastics.vue';
import CreatePlastic from '../views/private/Plastics/CreatePlastic.vue';
import Trace from '../views/private/Trace.vue';

// Views - Administration
import Configuration from '../views/private/Configuration.vue';
import CreateMaterialProp from '../views/private/Configuration/CreateMaterialProp.vue';
import CreateBatchProp from '../views/private/Configuration/CreateBatchProp.vue';
import ApiKeys from '../views/private/ApiKeys.vue';
import EditKey from '../views/private/ApiKeys/EditKey.vue';
import UserAdministration from '../views/private/UserAdministration.vue';
import CreateUser from '../views/private/UserAdministration/CreateUser.vue';
import EditUser from '../views/private/UserAdministration/EditUser.vue';
import StorageLocations from '../views/private/StorageLocations.vue';
import EditStorageSite from '../views/private/StorageLocations/EditStorageSite.vue';

// Store
import store from '../store';

// Administration router guard
function userIsAdministrator(_to, _from, next) {
  if (store.state.role === 0 || store.state.role === '0') {
    next();
  } else {
    next(_from);
  }
}

// Routes
const routes = [
  {
    path: '/',
    beforeEnter: (_to, _from, next) => {
      if (store.state.token === null) {
        next({ path: '/login' });
      } else {
        next({ path: '/private' });
      }
    },
  },
  {
    path: '/login',
    component: Public,
  },
  {
    path: '/private',
    component: Private,
    beforeEnter: function(_to, _from, next) {
      if (store.state.token === null) {
        next({ path: '/login' });
      } else {
        next();
      }
    },
    children: [
      {
        path: '',
        component: Dashboard,
      },
      {
        path: 'account',
        component: Account,
        beforeEnter: function(_to, _from, next) {
          if (store.state.user === 'openmts.guest') {
            next(_from);
          } else {
            next();
          }
        },
      },
      {
        path: 'sites',
        component: StorageSites,
      },
      {
        path: 'sites/environment',
        name: 'environment',
        component: Environment,
        props: true,
      },
      {
        path: 'inventory',
        name: 'inventory',
        component: Inventory,
        props: true,
      },
      {
        path: 'inventory/log',
        name: 'transactionLog',
        component: TransactionLog,
        props: true,
      },
      {
        path: 'inventory/create',
        component: CreateBatch,
      },
      {
        path: 'inventory/update',
        name: 'editBatch',
        component: EditBatch,
        props: true,
      },
      {
        path: 'materials',
        component: Materials,
      },
      {
        path: 'materials/create',
        component: CreateMaterial,
      },
      {
        path: 'materials/edit',
        name: 'editMaterial',
        component: EditMaterial,
        props: true,
      },
      {
        path: 'materials/details',
        name: 'materialDetails',
        component: MaterialDetails,
        props: true,
      },
      {
        path: 'plastics',
        name: 'plastics',
        component: Plastics,
        props: true,
      },
      {
        path: 'trace',
        name: 'trace',
        component: Trace,
        props: true,
      },
      {
        path: 'plastics/create',
        component: CreatePlastic,
      },
      {
        path: 'config',
        name: 'configuration',
        component: Configuration,
        props: true,
        beforeEnter: userIsAdministrator,
      },
      {
        path: 'config/create-material-prop',
        component: CreateMaterialProp,
        beforeEnter: userIsAdministrator,
      },
      {
        path: 'config/create-batch-prop',
        component: CreateBatchProp,
        beforeEnter: userIsAdministrator,
      },
      {
        path: 'users',
        name: 'users',
        component: UserAdministration,
        props: true,
        beforeEnter: userIsAdministrator,
      },
      {
        path: 'users/create',
        component: CreateUser,
        beforeEnter: userIsAdministrator,
      },
      {
        path: 'users/edit',
        name: 'editUser',
        component: EditUser,
        props: true,
        beforeEnter: userIsAdministrator,
      },
      {
        path: 'keys',
        name: 'keys',
        component: ApiKeys,
        props: true,
        beforeEnter: userIsAdministrator,
      },
      {
        path: 'keys/edit',
        name: 'editKey',
        component: EditKey,
        props: true,
        beforeEnter: userIsAdministrator,
      },
      {
        path: 'locations',
        name: 'locations',
        component: StorageLocations,
        props: true,
        beforeEnter: userIsAdministrator,
      },
      {
        path: 'locations/edit',
        name: 'editStoragSite',
        component: EditStorageSite,
        props: true,
        beforeEnter: userIsAdministrator,
      },
    ],
  },
];

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes,
});

export default router;
