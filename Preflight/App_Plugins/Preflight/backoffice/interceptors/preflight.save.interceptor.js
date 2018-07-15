﻿(() => {

    function interceptor($injector) {
        return {
            response: response => {
                try {
                    if (response.config.url.toLowerCase().indexOf('/umbracoapi/content/postsave') !== -1) {
                        if (response.data.notifications) {

                            const notification = response.data.notifications.filter(f => f.header === Umbraco.Sys.ServerVariables.preflight.contentFailedChecks)[0];

                            if (notification) {
                                const preflightResponse = JSON.parse(notification.message);

                                preflightResponse.introText =
                                    'Save cancelled due to failing Preflight checks. Please review the results below, update the content, and re-save.';
                                response.data.notifications = [];

                                // this is essentially the required bits from navigationService.executeMenuAction
                                // since we don't have a node, we can mock it instead
                                const dialogService = $injector.get('dialogService');
                                const appState = $injector.get('appState');
                                const $rootScope = $injector.get('$rootScope');

                                appState.setMenuState('dialogTitle', 'Preflight check');
                                appState.setGlobalState('navMode', 'dialog');
                                appState.setGlobalState('stickyNavigation', true);
                                appState.setGlobalState('showNavigation', true);
                                appState.setMenuState('showMenu', false);
                                appState.setMenuState('showMenuDialog', true);

                                dialogService.open({
                                    container: $('#dialog div.umb-modalcolumn-body'),
                                    scope: $rootScope.$new(),
                                    inline: true,
                                    show: true,
                                    iframe: false,
                                    modalClass: 'umb-dialog',
                                    template: '/App_Plugins/Preflight/backoffice/views/check.dialog.html',
                                    results: preflightResponse
                                });
                            }
                        }
                    }
                }
                catch (err) {
                    console.log(err.message);
                }

                return response;
            }
        };
    }

    angular.module('umbraco').factory('preflight.save.interceptor', ['$injector', interceptor]);

    angular.module('umbraco')
        .config(function ($httpProvider) {
            $httpProvider.interceptors.push('preflight.save.interceptor');
        });

})();