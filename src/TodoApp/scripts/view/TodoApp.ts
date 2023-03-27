// Copyright (c) Martin Costello, 2023. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { TodoClient } from '../client/TodoClient';
import { TodoItem } from '../models/TodoItem';
import { Classes } from './Classes';
import { Elements } from './Elements';
import { TodoElement } from './TodoElement';

export class TodoApp {
    private readonly client: TodoClient;
    private readonly elements: Elements;
    private readonly items: TodoElement[];

    constructor() {
        this.client = new TodoClient();
        this.elements = new Elements();
        this.items = [];
    }

    async initialize(): Promise<void> {
        if (!this.elements.createItemForm) {
            return;
        }

        this.elements.createItemForm.addEventListener('submit', (event) => {
            event.preventDefault();
            return false;
        });

        this.elements.createItemText.addEventListener('input', () => {
            if (this.elements.createItemText.value.length === 0) {
                this.disable(this.elements.createItemButton);
            } else {
                this.enable(this.elements.createItemButton);
            }
        });

        this.elements.createItemButton.addEventListener('click', () => {
            this.addNewItem();
        });

        const items = await this.client.getAll();

        items.forEach((item) => {
            this.createItem(item);
        });

        if (items.length > 0) {
            this.show(this.elements.itemTable);
        } else {
            this.show(this.elements.banner);
        }

        this.hide(this.elements.loader);

        window.setInterval(() => {
            this.items.forEach((item) => {
                item.refresh();
            });
        }, 30000);
    }

    async addNewItem(): Promise<void> {
        this.disable(this.elements.createItemButton);
        this.disable(this.elements.createItemText);

        try {
            const text = this.elements.createItemText.value;

            const id = await this.client.add(text);
            const item = await this.client.get(id);

            this.createItem(item);

            this.elements.createItemText.value = '';
            this.hide(this.elements.banner);
            this.show(this.elements.itemTable);
        } catch {
            this.enable(this.elements.createItemButton);
        } finally {
            this.enable(this.elements.createItemText);
            this.elements.createItemText.focus();
        }
    }

    createItem(item: TodoItem) {
        const element = this.elements.createNewItem(item);

        if (!item.isCompleted) {
            element.onComplete(async (id) => {
                await this.client.complete(id);
            });
        }

        element.onDeleting(async (id) => {
            await this.client.delete(id);
        });
        element.onDeleted(() => {
            if (this.elements.itemCount() < 1) {
                this.hide(this.elements.itemTable);
                this.show(this.elements.banner);
            }

            const index = this.items.findIndex(
                (item) => item.id() === element.id()
            );

            if (index > -1) {
                this.items.splice(index, 1);
            }
        });

        element.show();

        this.items.push(element);
    }

    private disable(element: Element) {
        element.setAttribute('disabled', '');
    }

    private enable(element: Element) {
        element.removeAttribute('disabled');
    }

    private hide(element: Element) {
        element.classList.add(Classes.hidden);
    }

    private show(element: Element) {
        element.classList.remove(Classes.hidden);
    }
}
