﻿(() => {

    function ctrl(notificationsService, preflightService) {

        this.content = preflightService.getHelpText();

        const getSettingByAlias = alias => this.settings.filter(v => v.alias === alias)[0];

        /**
         * 
         */
        const getSettings = () => {
            preflightService.getSettings()
                .then(resp => {
                    this.settings = resp.data.settings;
                    this.tabs = resp.data.tabs;

                    this.settings.forEach(v => {
                        if (v.view.indexOf('slider') !== -1) {
                            v.config = {
                                handle: 'round',
                                initVal1: v.alias === 'longWordSyllables' ? 5 : 65,
                                maxVal: v.alias === 'longWordSyllables' ? 10 : 100,
                                minVal: 0,
                                orientation: 'horizontal',
                                step: 1,
                                tooltip: 'always',
                                tooltipPosition: 'bottom',
                            };
                        } else if (v.view.indexOf('multipletextbox') !== -1) {

                            v.value = v.value.split(',').map(val => {
                                return { value: val };
                            }).sort((a, b) => a < b);

                            v.config = {
                                min: 0,
                                max: 0
                            };
                        }
                    });
                });
        };

        /**
         * 
         */
        this.saveSettings = () => {

            //const min = parseInt(getSettingByAlias('readabilityTargetMin').value);
            //const max = parseInt(getSettingByAlias('readabilityTargetMax').value);
            const min = 1;
            const max = 100;

            if (min < max) {

                if (min + 10 > max) {
                    notificationsService.warning('WARNING', 'Readability range is narrow');
                }

                // need to transform multitextbox values without digest
                // so must be a new object, not a reference
                const settingsToSave = JSON.parse(JSON.stringify(this.settings));

                settingsToSave.forEach(v => {
                    if (v.view.indexOf('multipletextbox') !== -1) {
                        v.value = v.value.map(o => o.value).join(',');
                    }
                });
                
                preflightService.saveSettings(settingsToSave)
                    .then(resp => {
                        resp.data
                            ? notificationsService.success('SUCCESS', 'Settings updated')
                            : notificationsService.error('ERROR', 'Unable to save settings');
                    });
            } else {
                notificationsService.error('ERROR',
                    'Unable to save settings - readability minimum cannot be greater than readability maximum');
            }
        };

        getSettings();
    }

    angular.module('umbraco').controller('preflight.settings.controller', ['notificationsService', 'preflightService', ctrl]);

})();