import axiosInstance from '../../utils/axiosInstance';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import './AccountDetails.scss';
import { Link } from 'react-router-dom';
import Popup from 'reactjs-popup';

const AccountDetails = () => {
  const [accounts, setAccounts] = useState({});

  const apiUrl = import.meta.env.VITE_API_URL;

  const nfObject = new Intl.NumberFormat('en-US');
  const today = new Date();
  const [dayX, setDay] = useState(today.getDate());
  const [monthX, setMonth] = useState(today.getMonth() + 1);

  const [XIRR, setXIRR] = useState({});

  const { year, id } = useParams();

  const monthsArray = [
    { month: 'January', value: 0 },
    { month: 'February', value: 1 },
    { month: 'March', value: 2 },
    { month: 'April', value: 3 },
    { month: 'May', value: 4 },
    { month: 'June', value: 5 },
    { month: 'July', value: 6 },
    { month: 'August', value: 7 },
    { month: 'September', value: 8 },
    { month: 'October', value: 9 },
    { month: 'November', value: 10 },
    { month: 'December', value: 11 },
  ];

  const getAccounts = async () => {
    try {
      const response = await axiosInstance.get(`${apiUrl}/Accounts`);
      return response.data;
    } catch (error) {
      console.error(`Error retrieving players: ${error}`);
    }
  };

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await getAccounts();
      setAccounts(response);
    };
    fetchAccounts();
  }, [id]);

  const getXIRR = async () => {
    try {
      const response = await axiosInstance.get(
        `${apiUrl}/Returns/individualaccountdata/${id}/year/${year}`,
      );
      return response.data;
    } catch (error) {
      console.error(`Error retrieving xirr: ${error}`);
    }
  };

  useEffect(() => {
    const fetchXIRR = async () => {
      const response = await getXIRR();
      setXIRR(response);
    };
    fetchXIRR();
  }, [id, year]);

  const postPayment = async (event) => {
    event.preventDefault();
    const { amount, dep, month, day } = event.target;
    const paymentObj = {
      accountId: id,
      amount: dep.value === 'true' ? amount.value : -amount.value,
      month: month.value,
      day: day.value,
      year: year,
    };
    const ID = await axiosInstance.post(`${apiUrl}/Update/add-payment`, paymentObj);
    const response = await getXIRR();
    setXIRR(response);
  };

  const postBalances = async (event) => {
    event.preventDefault();
    const balancesObj = {
      accountId: id,
      latestBalance: event.target.end.value,
      month: event.target.month.value,
      day: event.target.day.value,
      year: year,
    };
    console.log(balancesObj.month);
    const ID = await axiosInstance.put(`${apiUrl}/Update/update-balance`, balancesObj);
    const response = await getXIRR();
    setXIRR(response);
  };

  const deletePayment = async (paymentID) => {
    const ID = await axiosInstance.delete(
      `${apiUrl}/Update/delete-payment/${paymentID}`,
    );
    const response = await getXIRR();
    setXIRR(response);
  };

  // problem when you make 2026 or a new account, it starts empty so then page won't load
  if (!Object.keys(XIRR).length) {
    return <h2>Loading....</h2>;
  }

  return (
    <>
      <div className="Details__page">
        <div className="Details__page-header">
          <Link to={`/`}>
            <button className="Details__button">Back to overview</button>
          </Link>
          <p className="Details__last-updated">
            Last updated:{' '}
            {`${new Date(XIRR.balance.endDate).toLocaleDateString('en-GB', {
              day: 'numeric',
              month: 'long',
              year: 'numeric',
            })}`}
          </p>
        </div>
        <div className="Details__title-wrapper">
          <div className="Details__title-subwrap">
            <div className="Details__title-buttons">
              <Link
                to={
                  id > 1
                    ? `/accounts/${+id - 1}/${year}`
                    : `/accounts/${+id}/${year}`
                }
              >
                <button className="Details__button">Previous Account</button>
              </Link>
              <Link to={`/accounts/${+id + 1}/${year}`}>
                <button className="Details__button">Next Account</button>
              </Link>
            </div>
            <h2 className="Details__title">
              Account: {accounts?.[id - 1]?.name}
            </h2>
          </div>
          <div className="Details__title-subwrap">
            <div className="Details__title-buttons">
              <Link
                to={
                  year > 2021
                    ? `/accounts/${id}/${+year - 1}`
                    : `/accounts/${id}/${+year}`
                }
              >
                <button className="Details__button">Previous Year</button>
              </Link>
              <Link
                to={
                  year < 2026
                    ? `/accounts/${id}/${+year + 1}`
                    : `/accounts/${id}/${+year}`
                }
              >
                <button className="Details__button">Next Year</button>
              </Link>
            </div>
            <h2 className="Details__title">Year: {year}</h2>
          </div>
        </div>

        <div className="Details__Main">
          <div className="Details__First">
            <div className="Details__balance-list">
              <div className="Details__balance-list-header">
                <h2>Balances/payments list</h2>

                <Popup
                  trigger={
                    <button className="Details__addpayment-button">
                      Add payments
                    </button>
                  }
                  position="right center"
                >
                  {(close) => (
                    <div className="Details__popup">
                      <form
                        className="Details__popup-form"
                        onSubmit={(e) => {
                          e.preventDefault();
                          postPayment(e);
                          close();
                        }}
                      >
                        <div className="Details__popup-title">
                          <h2>Add Payment</h2>
                          <button
                            className="Details__popup-close"
                            onClick={close}
                            type="button"
                          >
                            &times;
                          </button>
                        </div>
                        <div className="Details__popup-form-item">
                          <label htmlFor="amount">Amount</label>
                          <input
                            name="amount"
                            id="amount"
                            type="number"
                            required
                          />
                        </div>
                        <div className="Details__popup-form-item">
                          <label htmlFor="dep">Deposit or Withdrawal</label>
                          <select name="dep" id="dep" required>
                            <option value="" disabled selected>
                              ---Choose an option---
                            </option>
                            <option value={true}>Deposit</option>
                            <option value={false}>Withdrawal</option>
                          </select>
                        </div>
                        <div className="Details__popup-form-item">
                          <label>Day / Month</label>
                          <div style={{ display: 'flex', gap: '10px' }}>
                            <select
                              name="day"
                              id="day"
                              value={dayX}
                              onChange={(e) => setDay(Number(e.target.value))}
                              required
                            >
                              <option value="" disabled>
                                ---Day---
                              </option>
                              {Array.from({ length: 31 }, (_, i) => i + 1).map(
                                (d) => (
                                  <option key={d} value={d}>
                                    {d}
                                  </option>
                                ),
                              )}
                            </select>
                            <select
                              name="month"
                              id="month"
                              value={monthX}
                              onChange={(e) => setMonth(Number(e.target.value))}
                              required
                            >
                              <option value="" disabled>
                                ---Month---
                              </option>
                              {monthsArray.map((m) => (
                                <option key={m.value} value={m.value + 1}>
                                  {monthsArray[m.value].month}
                                </option>
                              ))}
                            </select>
                          </div>
                        </div>
                        <button type="submit">Submit</button>
                      </form>
                    </div>
                  )}
                </Popup>
              </div>

              <div className="Details__payment-item">
                <h3>
                  1 January: Balance of £
                  {nfObject.format(XIRR.balance.startingBalance)}
                </h3>
              </div>

              {!XIRR.payments.length && <h3>No payments entered</h3>}

              {XIRR.payments &&
                XIRR.payments.map((payment) => {
                  return (
                    <>
                      <div key={payment.id} className="Details__payment-item">
                        <h3
                          className={
                            payment.amount >= 0
                              ? 'Details__deposited'
                              : 'Details__withdrawn'
                          }
                        >
                          {new Date(payment.date).toLocaleDateString('en-GB', {
                            day: 'numeric',
                            month: 'long',
                          })}
                          {': '}£{nfObject.format(Math.abs(payment.amount))}{' '}
                          {payment.amount >= 0 ? 'deposited' : 'withdrawn'}
                        </h3>

                        <button
                          className="Details__payment-button"
                          onClick={() => deletePayment(payment.id)}
                        >
                          Delete
                        </button>
                      </div>
                    </>
                  );
                })}

              <div className="Details__payment-item">
                <h3 className="Details__balance-item">
                  {`${new Date(XIRR.balance.endDate).toLocaleDateString(
                    'en-GB',
                    {
                      day: 'numeric',
                      month: 'long',
                    },
                  )}`}
                  : Balance of £{nfObject.format(XIRR.balance.endBalance)}
                </h3>
                <Popup
                  trigger={
                    <button className="Details__payment-button"> Edit</button>
                  }
                  position="right center"
                >
                  {(close) => (
                    <>
                      <div className="Details__popup">
                        <form
                          className="Details__popup-form"
                          onSubmit={(e) => {
                            e.preventDefault();
                            postBalances(e);
                            close();
                          }}
                        >
                          <div className="Details__popup-title">
                            <h2>Enter Balance Date and Amount</h2>
                            <button
                              className="Details__popup-close"
                              onClick={close}
                            >
                              &times;
                            </button>
                          </div>

                          <div className="Details__popup-form-item">
                            <label>Day</label>

                            <select
                              name="day"
                              id="day"
                              value={dayX}
                              onChange={(e) => setDay(Number(e.target.value))}
                            >
                              <option disabled selected>
                                ---Day---
                              </option>
                              {Array.from({ length: 31 }, (_, i) => i + 1).map(
                                (d) => (
                                  <option key={d} value={d}>
                                    {d}
                                  </option>
                                ),
                              )}
                            </select>
                            <label>Month</label>
                            <select
                              name="month"
                              id="month"
                              value={monthX}
                              onChange={(e) => setMonth(Number(e.target.value))}
                            >
                              <option disabled selected>
                                ---Month---
                              </option>
                              {monthsArray.map((m) => (
                                <option key={m.value} value={m.value + 1}>
                                  {monthsArray[m.value].month}
                                </option>
                              ))}
                            </select>
                          </div>
                          <label>Amount</label>
                          <input
                            name="end"
                            id="end"
                            type="number"
                            defaultValue={XIRR.balance.endBalance || 0}
                          />

                          <button type="submit">Submit</button>
                        </form>
                      </div>
                    </>
                  )}
                </Popup>
              </div>
            </div>
          </div>

          <div className="Details__Analysis">
            <h2>Payments overview</h2>
            <div className="Details__Analysis-wrapper">
              <p className="Details__Analysis-property">Total deposits </p>
              <p>
                {XIRR.totalDeposits
                  ? `£${nfObject.format(XIRR.totalDeposits)}`
                  : '£0'}{' '}
                (
                {(
                  (XIRR.totalDeposits / XIRR.balance.startingBalance) *
                  100
                ).toFixed(2)}
                %)
              </p>
            </div>
            <div className="Details__Analysis-wrapper">
              <p className="Details__Analysis-property">Total withdrawals</p>
              <p>
                {' '}
                {XIRR.totalWithdrawals
                  ? `£${nfObject.format(Math.abs(XIRR.totalWithdrawals))}`
                  : '£0'}{' '}
                (
                {(
                  (XIRR.totalWithdrawals / XIRR.balance.startingBalance) *
                  100
                ).toFixed(2)}
                %)
              </p>
            </div>
            <div className="Details__Analysis-wrapper">
              <p className="Details__Analysis-property">Total net payments </p>
              <p>
                {XIRR.netDeposits
                  ? `£${nfObject.format(XIRR.netDeposits)}`
                  : '£0'}{' '}
                (
                {(
                  (XIRR.netDeposits / XIRR.balance.startingBalance) *
                  100
                ).toFixed(2)}
                %)
              </p>
            </div>
            <h2>Returns overview</h2>
            <div className="Details__Analysis-wrapper">
              <p className="Details__Analysis-property">Return (XIRR) </p>
              <p>{XIRR.xirr}%</p>
            </div>
            <div className="Details__Analysis-wrapper">
              <p className="Details__Analysis-property">
                Growth (net payments + XIRR)
              </p>
              <p>
                {(
                  (XIRR.netDeposits / XIRR.balance.startingBalance) * 100 +
                  XIRR.xirr
                ).toFixed(2)}
                %
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="Details__Editing">
        <div className="Details__payments-wrapper"></div>
      </div>
    </>
  );
};

export default AccountDetails;
