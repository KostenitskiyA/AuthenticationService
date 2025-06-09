import React from 'react';
import { Outlet } from 'react-router-dom';

import styles from './Layout.module.scss';

const Layout = () => {
    return (
        <div className={styles.Layout}>
            <Outlet />
        </div>
    );
};

export default Layout;
