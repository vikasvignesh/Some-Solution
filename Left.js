import React from 'react';
import './Left.css';
import ReactDOM from 'react-dom';
import data from './innovacate.json';
import $ from 'jquery'
import { Scrollbars } from 'react-custom-scrollbars';



  
function Show(element){

}

class Left extends React.Component
{

 state={
		visible:false
		};
		
fn_select_sub_region=(check)=>{
		
		
		}

subcategory=(element, option, id)=>{
//alert(obj.name);
	var subid="#leftsubinncat_"+id;

const {visible} = this.state;

this.setState({visible:!visible});


console.log(this.state.visible);
if(this.state.visible)
	{
	$(subid).css("display", "none");
	}
	else
	{
	$(subid).css("display", "block");
	}

    }
func=(fn)=>{
alert(fn);
}

render(){
return (
	
	<div id="mainContainerDiv">
    <div class="containermain">
        <form id="PrepareChart" method="post" action="Index">
			
            <div id="leftcontainer">
				<Scrollbars className="scroll">
                <div className="chartsearchdiv">
				
                    <div id="leftmenuschartfilter">

                        
                            <div id="leftmenuschartfilter">
                                <div class="showsearchmenu">

                                    <div className="leftmenu_mode">
                                        <ul className="breadcrumb_ss">
                                            <li><a href="javascript:;" className="category_mode active" onclick="Showcontent('category_mode', 'country_mode')">Category</a></li>
                                            <li><a href="javascript:;" className="country_mode" onClick={() =>Show(this)}>Country</a></li>
                                        </ul>
                                    </div>

                                    <div id="category">

                                        <div class="SearchSubTitle floatleft">

                                        </div>
                                        <div></div>
                                        <div class="singlecont">
                                            <div>
                                                { data.map(((market, index) =>

                                                <div class="Categories">
                                                    <div class="Image_plus">
                                                        <img src="plusgrey.svg" class="Image" onClick={() =>this.subcategory(this,'child',index)}/>
                                                    </div>

                                                    <label className="region_content width_80per">
                                                        <input type="checkbox" className="cat_select" id={ "check_" +index} name="MarketCategory" onClick={()=>this.fn_select_sub_region(this)}/> {market.name}

                                                    </label>

                                                    <div class="Sub_categories" id={ "leftsubinncat_" +index}>

                                                        { market.sub.map(d => (

                                                        <div class="suboption">

                                                            <label class="subleftmenu WidthAuto sub_cat2_333">
                                                                <input id="" class="sub_cat_2" type="checkbox" value="@subitem.Id" name="marketsubcategory" onclick="fn_select_sub_region(this,'child');" />{d.name}
                                                            </label>
                                                        </div>
                                                        )) }

                                                    </div>
                                                </div>

                                                )) }
                                            </div>

                                        </div>
											

                                    </div>

                                </div>
						 
                       

                        </div>

                    </div>
				
                </div>
</Scrollbars>
            </div>
			 
        </form>
    </div>
</div>
	
);
	


}

}

export default Left;