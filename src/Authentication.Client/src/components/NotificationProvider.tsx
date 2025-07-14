import React, { createContext, useCallback, useContext, useRef } from 'react';
import { Messages, MessagesMessage } from 'primereact/messages';
import { Toast } from 'primereact/toast';

type NotificationContextType = {
    addMessage: (message: MessagesMessage) => void;
    clearMessages: () => void;
};

const NotificationContext = createContext<NotificationContextType | null>(null);

export const useNotificationContext = (): NotificationContextType => {
    const context = useContext(NotificationContext);

    if (!context) {
        throw new Error('useNotification must be used within a NotificationProvider');
    }

    return context;
};

export const NotificationProvider = ({ children }: { children: React.ReactNode }) => {
    const messagesRef = useRef<Messages>(null);

    const addMessage = useCallback((message: MessagesMessage) => {
        messagesRef.current?.show(message);
    }, []);

    const clearMessages = useCallback(() => {
        messagesRef.current?.clear();
    }, []);

    return (
        <NotificationContext.Provider value={{ addMessage, clearMessages }}>
            <>
                {children}
                <Toast ref={messagesRef} />
            </>
        </NotificationContext.Provider>
    );
};
