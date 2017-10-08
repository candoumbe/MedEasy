import * as React from "react";
import { Form } from "./../restObjects/Form";
import { FormField } from "./../restObjects/FormField";
import { FormFieldType } from "./../restObjects/FormFieldType";
import { FormFieldComponent } from "./FormFieldComponent";
import * as ReactRouter from "react-router";
import * as LinQ from "linq";

enum ModalComponentWidth {
    Small,
    Normal,
    Large,
    XLarge
}


interface ModalComponentProps {
    title: string,
    closeOnEscape: boolean | undefined,
    width: ModalComponentWidth | undefined
}


export class ModalComponent extends React.Component<ModalComponentProps, any> {

    /**
     * Builds a new ModalComponent instance.
     * @param {ModalComponentProps} props properties of the modal
     */
    public constructor(props: ModalComponentProps) {
        super(props);
    }


    public render(): JSX.Element | null {
        let width: string = "";
        if (this.props.width) {
            switch (this.props.width) {
                //case ModalComponentWidth.Small:
                //    width = "modal"
                //    break;
                default:
                    break;
            }
        }
        return (
            <div className={`modal `}>

            </div>
        )
    }
}