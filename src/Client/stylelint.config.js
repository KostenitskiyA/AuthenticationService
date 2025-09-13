export default {
    extends: ['stylelint-config-standard-scss'],
    plugins: ['stylelint-order'],
    rules: {
        'order/properties-order': [
            [
                {
                    groupName: 'Positioning',
                    properties: ['position', 'top', 'right', 'bottom', 'left', 'z-index'],
                },
                {
                    groupName: 'Box Model',
                    properties: ['display', 'width', 'height', 'margin', 'padding', 'border'],
                },
                {
                    groupName: 'Typography',
                    properties: ['font-size', 'font-weight', 'line-height', 'color', 'text-align'],
                },
                {
                    groupName: 'Visual',
                    properties: ['background', 'box-shadow', 'opacity'],
                },
                {
                    groupName: 'Misc',
                    properties: ['transition', 'cursor'],
                },
            ],
            { unspecified: 'bottomAlphabetical' },
        ],
        'at-rule-no-unknown': null,
        'scss/at-rule-no-unknown': true,
    },
};
