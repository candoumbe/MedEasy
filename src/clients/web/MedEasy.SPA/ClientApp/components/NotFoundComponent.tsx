import * as React from "react";

export class NotFoundComponent extends React.PureComponent<{text? : string | JSX.Element}, any>{

    public render() {

        return (
            <div>
                {this.props.text}
            </div>
            );
    }
}